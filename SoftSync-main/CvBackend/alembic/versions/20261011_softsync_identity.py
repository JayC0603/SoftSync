"""Map immutable SoftSync identities to CV users."""
from alembic import op
import sqlalchemy as sa

revision = "20261011_softsync_identity"
down_revision = "20260722_password_reset"
branch_labels = None
depends_on = None


def upgrade():
    op.create_table("softsync_identities",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("issuer", sa.String(255), nullable=False),
        sa.Column("subject", sa.String(255), nullable=False),
        sa.Column("user_id", sa.Integer(), sa.ForeignKey("users.id"), nullable=False, unique=True),
        sa.Column("email", sa.String(255), nullable=False, server_default=""),
        sa.Column("username", sa.String(255), nullable=False, server_default=""),
        sa.UniqueConstraint("issuer", "subject", name="uq_softsync_identity"))


def downgrade():
    op.drop_table("softsync_identities")
