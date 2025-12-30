# =============================================================================
# SECRETS DIRECTORY - CarDealer Microservices
# =============================================================================
# 
# This directory contains secret files for Docker Secrets.
# 
# ⚠️ NEVER commit actual secret files to version control! ⚠️
# 
# This directory should contain:
# - db_password.txt
# - jwt_secret_key.txt
# - rabbitmq_password.txt
# - redis_password.txt
# - sendgrid_api_key.txt
# - twilio_account_sid.txt
# - twilio_auth_token.txt
# - firebase_service_account.json
# - google_client_id.txt
# - google_client_secret.txt
# - microsoft_client_id.txt
# - microsoft_client_secret.txt
# - aws_access_key.txt
# - aws_secret_key.txt
# - azure_blob_connection_string.txt
# - elasticsearch_password.txt
# 
# Each .txt file should contain ONLY the secret value (no newline at end).
# 
# Example for db_password.txt:
# ```
# your_secure_password_here
# ```
# 
# For production, use a secrets manager like:
# - HashiCorp Vault
# - AWS Secrets Manager
# - Azure Key Vault
# - GCP Secret Manager
# =============================================================================
