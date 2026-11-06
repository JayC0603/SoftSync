from sqlalchemy import Column, ForeignKey, Integer, String, UniqueConstraint

from app.db.base import Base


class SoftSyncIdentity(Base):
    """Bind the immutable SoftSync account ID, never an email, to a CV account."""

    __tablename__ = "softsync_identities"
    __table_args__ = (UniqueConstraint("issuer", "subject", name="uq_softsync_identity"),)

    id = Column(Integer, primary_key=True)
    issuer = Column(String(255), nullable=False)
    subject = Column(String(255), nullable=False)
    user_id = Column(Integer, ForeignKey("users.id"), unique=True, nullable=False)
    email = Column(String(255), nullable=False, default="")
    username = Column(String(255), nullable=False, default="")
