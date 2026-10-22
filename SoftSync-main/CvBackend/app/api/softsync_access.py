from fastapi import Depends, HTTPException, Request
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from sqlalchemy.orm import Session

from app.core.config import settings
from app.core.security import decode_jwt_token
from app.db.session import get_db
from app.models.user import TokenBlacklist, User

bearer = HTTPBearer(auto_error=False)


def require_cv_access(request: Request, credentials: HTTPAuthorizationCredentials | None = Depends(bearer),
                      db: Session = Depends(get_db)) -> None:
    # Preserve standalone deployments; connected SoftSync deployments require a
    # validated CV session for every operation in the recruitment workspace.
    if not settings.softsync_shared_secret or request.url.path.endswith("/health"):
        return
    if not credentials:
        raise HTTPException(401, "Sign in through SoftSync to use CV tools")
    try:
        token = decode_jwt_token(credentials.credentials)
        if token.get("type") != "access" or not token.get("jti"):
            raise ValueError("Invalid access token")
        user_id = int(token["sub"])
    except (ValueError, KeyError, TypeError) as exc:
        raise HTTPException(401, "Invalid or expired CV session") from exc
    user = db.get(User, user_id)
    if not user or not user.is_active or db.query(TokenBlacklist).filter_by(jti=token["jti"]).first():
        raise HTTPException(401, "Invalid or expired CV session")
