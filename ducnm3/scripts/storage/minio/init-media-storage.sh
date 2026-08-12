#!/bin/sh
set -eu

alias_name="bootstrap"
policy_name="media-service-storage"
policy_file="/tmp/media-service-storage-policy.json"

mc alias set \
  "$alias_name" \
  "$MINIO_ENDPOINT" \
  "$MINIO_ROOT_USER" \
  "$MINIO_ROOT_PASSWORD"

for bucket in \
  "$MINIO_IMAGE_BUCKET" \
  "$MINIO_VIDEO_BUCKET" \
  "$MINIO_DOCUMENT_BUCKET" \
  "$MINIO_AUDIO_BUCKET" \
  "$MINIO_OTHER_BUCKET"
do
  mc mb --ignore-existing "$alias_name/$bucket"
done

cat > "$policy_file" <<POLICY
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetBucketLocation",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::${MINIO_IMAGE_BUCKET}",
        "arn:aws:s3:::${MINIO_VIDEO_BUCKET}",
        "arn:aws:s3:::${MINIO_DOCUMENT_BUCKET}",
        "arn:aws:s3:::${MINIO_AUDIO_BUCKET}",
        "arn:aws:s3:::${MINIO_OTHER_BUCKET}"
      ]
    },
    {
      "Effect": "Allow",
      "Action": [
        "s3:DeleteObject",
        "s3:GetObject",
        "s3:PutObject"
      ],
      "Resource": [
        "arn:aws:s3:::${MINIO_IMAGE_BUCKET}/*",
        "arn:aws:s3:::${MINIO_VIDEO_BUCKET}/*",
        "arn:aws:s3:::${MINIO_DOCUMENT_BUCKET}/*",
        "arn:aws:s3:::${MINIO_AUDIO_BUCKET}/*",
        "arn:aws:s3:::${MINIO_OTHER_BUCKET}/*"
      ]
    }
  ]
}
POLICY

mc admin policy create "$alias_name" "$policy_name" "$policy_file"

if mc admin user info "$alias_name" "$MINIO_APP_ACCESS_KEY" >/dev/null 2>&1
then
  mc admin user remove "$alias_name" "$MINIO_APP_ACCESS_KEY"
fi

mc admin user add \
  "$alias_name" \
  "$MINIO_APP_ACCESS_KEY" \
  "$MINIO_APP_SECRET_KEY"
mc admin policy attach \
  "$alias_name" \
  "$policy_name" \
  --user "$MINIO_APP_ACCESS_KEY"
