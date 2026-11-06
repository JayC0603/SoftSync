import hashlib
from datetime import UTC, datetime, timedelta

import jwt
from fastapi import APIRouter, Depends, HTTPException
from pydantic import BaseModel, Field
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from app.core.config import settings
from app.core.security import create_jwt_token
from app.db.session import get_db
from app.models.softsync_identity import SoftSyncIdentity
from app.models.user import TokenBlacklist, User

router = APIRouter()


class ExchangeRequest(BaseModel):
    assertion: str = Field(min_length=1, max_length=8192)


@router.post("/exchange")
def exchange(payload: ExchangeRequest, db: Session = Depends(get_db)) -> dict:
    secret = settings.softsync_shared_secret
    if len(secret.encode("utf-8")) < 32:
        raise HTTPException(503, "SoftSync integration is not configured")
    try:
        claims = jwt.decode(
            payload.assertion, secret, algorithms=["HS256"],
            issuer=settings.softsync_issuer, audience="softsync-cv",
            options={"require": ["iss", "aud", "sub", "iat", "exp", "jti", "type"]},
        )
        if claims["type"] != "softsync_assertion" or not 0 < claims["exp"] - claims["iat"] <= 60:
            raise ValueError("Invalid assertion lifetime or type")
        for key in ["sub", "jti"]:
            if not isinstance(claims[key], str) or not 0 < len(claims[key]) <= 64:
                raise ValueError("Invalid identity")
        if any(not isinstance(claims.get(key, ""), str) or len(claims.get(key, "")) > 255 for key in ["name", "email", "username"]):
            raise ValueError("Invalid profile")
    except (jwt.InvalidTokenError, ValueError, TypeError) as exc:
        raise HTTPException(401, "Invalid or expired SoftSync assertion") from exc

    if db.query(TokenBlacklist).filter(TokenBlacklist.jti == claims["jti"]).first():
        raise HTTPException(401, "SoftSync assertion was already used")
    identity = db.query(SoftSyncIdentity).filter_by(issuer=claims["iss"], subject=claims["sub"]).first()
    if identity:
        user = db.get(User, identity.user_id)
        if not user or not user.is_active:
            raise HTTPException(403, "CV account is inactive")
    else:
        # Do not silently link an existing local/Google account by email.
        digest = hashlib.sha256(f'{claims["iss"]}\0{claims["sub"]}'.encode()).hexdigest()
        user = User(username=f"ss_{digest[:40]}", email=f"{digest}@softsync.invalid",
                    full_name=claims.get("name") or claims.get("username") or "SoftSync",
                    role="hr", is_active=True, auth_provider="softsync", hashed_password=None)
        db.add(user)
        try:
            db.flush()
        except IntegrityError as exc:
            db.rollback()
            raise HTTPException(409, "Account connection is already in progress. Retry.") from exc
        identity = SoftSyncIdentity(issuer=claims["iss"], subject=claims["sub"], user_id=user.id)
        db.add(identity)
    identity.email = claims.get("email", "")
    identity.username = claims.get("username", "")
    user.full_name = claims.get("name") or user.full_name
    db.add(TokenBlacklist(jti=claims["jti"], token_type="softsync_assertion", user_id=user.id,
                          expires_at=datetime.fromtimestamp(claims["exp"], UTC).replace(tzinfo=None)))
    try:
        db.commit()
    except IntegrityError as exc:
        db.rollback()
        raise HTTPException(409, "Account connection is already in progress. Retry.") from exc
    expires = timedelta(minutes=settings.access_token_expire_minutes)
    token = create_jwt_token(subject=str(user.id), token_type="access", expires_delta=expires)
    return {"access_token": token, "expires_at": int((datetime.now(UTC) + expires).timestamp())}
