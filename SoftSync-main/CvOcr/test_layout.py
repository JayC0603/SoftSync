import unittest
from layout import reading_order


def block(text, x, y, right, bottom):
    return {"text": text, "box": [x, y, right, bottom]}


class LayoutTests(unittest.TestCase):
    def test_columns_are_not_interleaved_and_header_stays_first(self):
        blocks = [block("right2", 300, 180, 450, 195),
                  block("left1", 10, 100, 150, 115),
                  block("header", 10, 10, 450, 30),
                  block("right1", 300, 100, 450, 115),
                  block("left2", 10, 180, 150, 195)]
        self.assertEqual([b["text"] for b in reading_order(blocks)],
                         ["header", "left1", "left2", "right1", "right2"])

    def test_empty_and_one_column(self):
        self.assertEqual(reading_order([]), [])
        blocks = [block("bottom", 10, 100, 100, 120), block("top", 10, 10, 100, 30)]
        self.assertEqual([b["text"] for b in reading_order(blocks)], ["top", "bottom"])


if __name__ == "__main__":
    unittest.main()
