from datetime import UTC, datetime
from uuid import uuid4

import jwt
import pytest
from fastapi import Depends, FastAPI
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from sqlalchemy.pool import StaticPool

from app.api.softsync_access import require_cv_access
from app.api.v1.endpoints.auth import router as auth_router
from app.api.v1.endpoints.softsync import router as softsync_router
from app.core.config import settings
from app.core.security import decode_jwt_token
from app.db.base import Base
from app.db.session import get_db
from app.models.softsync_identity import SoftSyncIdentity
from app.models.user import User

SECRET = "test-only-softsync-key-at-least-32-bytes"


@pytest.fixture
def system(monkeypatch):
    monkeypatch.setattr(settings, "softsync_shared_secret", SECRET)
    monkeypatch.setattr(settings, "softsync_issuer", "softsync-tests")
    engine = create_engine("sqlite://", connect_args={"check_same_thread": False}, poolclass=StaticPool)
    Base.metadata.create_all(engine)
    factory = sessionmaker(bind=engine)

    def database():
        with factory() as db:
            yield db

    app = FastAPI()
    app.dependency_overrides[get_db] = database
    app.include_router(auth_router, prefix="/api/auth")
    app.include_router(softsync_router, prefix="/api/auth/softsync")

    @app.get("/api/v1/protected", dependencies=[Depends(require_cv_access)])
    def protected():
        return {"ok": True}

    with TestClient(app) as client:
        yield client, factory
    engine.dispose()


def assertion(subject="42", **overrides):
    now = int(datetime.now(UTC).timestamp())
    payload = dict(iss="softsync-tests", aud="softsync-cv", sub=subject,
                   iat=now, exp=now + 60, jti=uuid4().hex, type="softsync_assertion",
                   email="person@example.com", name="SoftSync User", username="learner")
    payload.update(overrides)
    return jwt.encode(payload, SECRET, algorithm="HS256")


def connect(client, token):
    return client.post("/api/auth/softsync/exchange", json={"assertion": token})


def test_one_account_profile_and_stable_identity(system):
    client, factory = system
    first = connect(client, assertion())
    assert first.status_code == 200
    token = first.json()["access_token"]
    profile = client.get("/api/auth/me", headers={"Authorization": f"Bearer {token}"})
    assert profile.status_code == 200
    assert profile.json()["email"] == "person@example.com"
    assert profile.json()["full_name"] == "SoftSync User"
    assert profile.json()["auth_provider"] == "softsync"
    second = connect(client, assertion(email="updated@example.com", name="Updated Name"))
    assert second.status_code == 200
    assert decode_jwt_token(second.json()["access_token"])["sub"] == decode_jwt_token(token)["sub"]
    with factory() as db:
        assert db.query(SoftSyncIdentity).count() == 1


def test_no_automatic_email_account_linking(system):
    client, factory = system
    with factory() as db:
        db.add(User(username="local-user", email="person@example.com", full_name="Local User",
                    auth_provider="local", role="hr", is_active=True))
        db.commit()
    first = connect(client, assertion())
    second = connect(client, assertion(subject="43"))
    assert first.status_code == second.status_code == 200
    assert decode_jwt_token(first.json()["access_token"])["sub"] != decode_jwt_token(second.json()["access_token"])["sub"]
    with factory() as db:
        assert db.query(User).count() == 3


@pytest.mark.parametrize("claims", [
    {"exp": 1, "iat": 0}, {"aud": "wrong-app"}, {"iss": "untrusted"},
    {"type": "access"}, {"exp": int(datetime.now(UTC).timestamp()) + 3600},
])
def test_rejects_invalid_assertions(system, claims):
    client, _ = system
    assert connect(client, assertion(**claims)).status_code == 401


def test_replay_and_tampered_signature(system):
    client, _ = system
    token = assertion()
    assert connect(client, token).status_code == 200
    assert connect(client, token).status_code == 401
    unsigned = token.rsplit(".", 1)[0]
    assert connect(client, unsigned + ".invalid").status_code == 401


def test_protected_tools_require_live_access_token(system):
    client, factory = system
    assert client.get("/api/v1/protected").status_code == 401
    token = connect(client, assertion()).json()["access_token"]
    headers = {"Authorization": f"Bearer {token}"}
    assert client.get("/api/v1/protected", headers=headers).status_code == 200
    with factory() as db:
        user = db.get(User, int(decode_jwt_token(token)["sub"]))
        user.is_active = False
        db.commit()
    assert client.get("/api/v1/protected", headers=headers).status_code == 401
    assert connect(client, assertion()).status_code == 403


def test_phone_only_account_and_unconfigured_bridge(system, monkeypatch):
    client, _ = system
    token = connect(client, assertion(email="")).json()["access_token"]
    assert client.get("/api/auth/me", headers={"Authorization": f"Bearer {token}"}).json()["email"] == ""
    monkeypatch.setattr(settings, "softsync_shared_secret", "")
    assert connect(client, assertion()).status_code == 503
