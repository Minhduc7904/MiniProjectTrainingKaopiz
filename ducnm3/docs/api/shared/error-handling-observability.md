# Error Handling and Observability

## Error handling

Use global `ExceptionHandlingMiddleware` and the shared response contract in [error-format.md](error-format.md).

```text
ValidationException
    → 400

NotFoundException
    → 404

ConflictException
    → 409

Unhandled Exception
    → 500

Database unavailable
    → 503 DATABASE_UNAVAILABLE

Downstream service unavailable at API Gateway
    → 503 SERVICE_UNAVAILABLE
```

Never expose stack traces, connection strings, internal SQL, or secrets.

## Logging and observability

Use structured logs (for example, Serilog) with:

```text
Request
Response Status
Elapsed Time
CorrelationId

Batch Job Start
Batch Number
Batch Size
Success Count
Failure Count
Retry
Elapsed Time

Database slow operation
Unhandled exception
```

Example:

```text
JobId=123
Batch=15
Records=500
Success=493
Failed=7
ElapsedMs=836
```
