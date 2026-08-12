# Shared Error Format

The canonical success, error, cursor-pagination, and offset-pagination envelopes are defined in [response-format.md](response-format.md).

Use its `error` and `meta.traceId` fields for every JSON error response. Validation field errors belong in `error.details`; do not add endpoint-specific top-level response fields.
