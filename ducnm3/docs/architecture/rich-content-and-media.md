# Rich Content and Media Design

## Ownership boundary

Media Service alone owns:

- MinIO credentials and object upload, download, and deletion.
- `media_objects` metadata and `media_usages` links.
- File validation, size limits, MIME allowlist, and content delivery URL.

Course Service owns Course/ Lesson Markdown. Notification Service owns notification Markdown. Neither service may use MinIO SDK or query the Media Service database.

## Markdown fields

| Owner | Field | Purpose |
|---|---|---|
| Course | `description_markdown` | Rich description of a Course |
| Lesson | `content_markdown` | Rich lesson content |
| Notification Job | `body_markdown` | Template used to create bulk notifications |
| Notification | `body_markdown` | Rendered inbox content for one Student |

The stored value is Markdown source. Raw HTML, JavaScript, inline event handlers, and unsafe URL schemes must be stripped by the renderer.

## Media lifecycle

1. Client uploads multipart data to Media Service.
2. Media Service validates the file, writes it to MinIO, and creates `media_objects`.
3. Client receives `mediaId` and a content URL, then includes the URL in Markdown, for example `![Course image](/api/media/{mediaId}/content)`.
4. Course or Notification Service saves the Markdown source.
5. The owner service calls Media Service to create `media_usages` for every thumbnail, attachment, or embed.
6. When content removes a reference, the owner service removes the matching usage. Media Service can garbage-collect media that has no active usage after a retention period.

Course does not store `thumbnail_media_id`. To render a thumbnail, it asks Media Service for the active usage with `ownerService=COURSE`, `ownerType=COURSE_THUMBNAIL`, and `ownerId={courseId}`.

## Usage semantics

- `THUMBNAIL`: exactly one active `COURSE_THUMBNAIL` usage per Course; it is the source of truth for the display image.
- `EMBED`: media rendered inline in Markdown.
- `ATTACHMENT`: downloadable media associated with the owner but not rendered inline.

`owner_service`, `owner_type`, and `owner_id` in `media_usages` are logical references across services. They deliberately are not database foreign keys.

## Notification delivery

- A single notification writes one `notifications` row for one recipient.
- A bulk job snapshots recipients and writes `notification_job_items`.
- The worker creates one `notifications` row per successful item.
- The worker uses `(notification_job_id, recipient_student_id)` as an idempotency key, preventing a second inbox item after retry or restart.
