import base64
import os
import unittest
from types import SimpleNamespace
from unittest.mock import patch

from fastapi.testclient import TestClient
import app as service


class ApiTests(unittest.TestCase):
    def setUp(self):
        self.environment = patch.dict(os.environ, {"CV_OCR_API_KEY": "test-only"})
        self.environment.start()
        # Do not start lifespan: these are HTTP boundary tests, not real OCR tests.
        self.client = TestClient(service.app)
        self.headers = {"Authorization": "Bearer test-only"}
        self.payload = {"pdf": base64.b64encode(b"%PDF-synthetic").decode(), "pages": [1]}

    def tearDown(self):
        self.client.close()
        self.environment.stop()

    def test_authentication_and_invalid_input(self):
        self.assertEqual(self.client.post("/ocr", json=self.payload).status_code, 401)
        for pages in ([0], [1, 1], [True], list(range(1, 12))):
            self.payload["pages"] = pages
            self.assertEqual(self.client.post("/ocr", headers=self.headers, json=self.payload).status_code, 422)

    def test_warming_up_and_success_contract(self):
        self.assertEqual(self.client.post("/ocr", headers=self.headers, json=self.payload).status_code, 503)
        expected = {"pages": [{"page": 1, "text": "Synthetic OCR text", "confidence": .9}]}
        with patch.object(service, "engine", object()), patch.object(service, "recognize", return_value=expected) as recognize:
            response = self.client.post("/ocr", headers=self.headers, json=self.payload)
            self.assertEqual(response.status_code, 200)
            self.assertEqual(response.json(), expected)
            recognize.assert_called_once_with(b"%PDF-synthetic", [1])

    def test_worker_renders_only_requested_pages_and_reads_paddle_result(self):
        import numpy as np
        rendered = []

        class Bitmap:
            def to_pil(self):
                return SimpleNamespace(convert=lambda mode: np.zeros((32, 32, 3), dtype=np.uint8))
            def close(self):
                pass

        class Page:
            def get_size(self):
                return (600, 800)
            def render(self, scale):
                return Bitmap()
            def close(self):
                pass

        class Document:
            def __enter__(self):
                return self
            def __exit__(self, *args):
                pass
            def __len__(self):
                return 3
            def __getitem__(self, index):
                rendered.append(index)
                return Page()

        raw = {"res": {"rec_texts": ["Second", "First"], "rec_scores": [.9, .8],
                       "rec_polys": [[[10, 50], [100, 50], [100, 60], [10, 60]],
                                     [[10, 10], [100, 10], [100, 20], [10, 20]]]}}
        fake = SimpleNamespace(predict=lambda image: [SimpleNamespace(json=raw)])
        with patch.dict("sys.modules", {"pypdfium2": SimpleNamespace(PdfDocument=lambda pdf: Document())}), patch.object(service, "engine", fake):
            result = service.recognize(b"synthetic", [2])
        self.assertEqual(rendered, [1])
        self.assertEqual(result["pages"], [{"page": 2, "text": "First\nSecond", "confidence": .8}])
        self.assertFalse(service.worker_lock.locked())


if __name__ == "__main__":
    unittest.main()
