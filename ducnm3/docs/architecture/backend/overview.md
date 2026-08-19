# Tổng quan kiến trúc Backend

Backend dùng ASP.NET Core microservices, YARP Gateway, MySQL, RabbitMQ và MinIO. Gateway là public entry point; client không gọi service nội bộ trực tiếp.

~~~mermaid
flowchart LR
  Client --> Gateway[YARP API Gateway]
  Gateway --> Course[Course Service]
  Gateway --> Student[Student Service]
  Gateway --> Media[Media Service]
  Gateway --> Notification[Notification Service]
  Gateway --> Scheduler[Scheduler Service]
  Course --> CourseDb[(lms_course_db)]
  Student --> StudentDb[(lms_student_db)]
  Media --> MediaDb[(lms_media_db)]
  Media --> MinIO[(MinIO)]
  Notification --> NotificationDb[(lms_notification_db)]
  Scheduler --> SchedulerDb[(lms_scheduler_db)]
  Course --- MQ[RabbitMQ]
  Student --- MQ
  Media --- MQ
  Notification --- MQ
  Scheduler --- MQ
~~~

## Ownership

Mỗi service sở hữu database riêng. ID của service khác là logical reference; không có foreign key hoặc truy vấn chéo database. Chỉ [Media Service](media-service/architecture.md) sở hữu MinIO.

| Service | Ownership |
| --- | --- |
| Course | Course, Lesson, Enrollment, LessonProgress |
| Student | Student và profile |
| Media | Metadata, usage, upload/content và MinIO |
| Notification | Notification đơn, batch, batch item, inbox |
| Scheduler | Định nghĩa job và lịch sử job run |

## Đã triển khai hiện tại

Gateway có reverse proxy, CORS, health/info endpoint và Swagger aggregation. Mỗi service API chạy SQL migration khi khởi động, có health endpoint và cấu hình MassTransit. Media và Notification có Worker riêng.

## Định hướng/chưa triển khai

Scheduler chưa có API nghiệp vụ, Cron evaluation, polling/claiming hoặc job handler.




.
├── BuildingBlocks
│   ├── BuildingBlocks.Communication.UnitTests
│   │   ├── BuildingBlocks.Communication.UnitTests.csproj
│   │   └── MessagingFoundationTests.cs
│   ├── BuildingBlocks.Contracts
│   │   ├── Api
│   │   │   ├── ApiConstants.cs
│   │   │   ├── ApiContracts.cs
│   │   │   ├── ApiException.cs
│   │   │   └── ApiRoutes.cs
│   │   ├── BuildingBlocks.Contracts.csproj
│   │   ├── Health
│   │   │   ├── IDatabaseHealthProbe.cs
│   │   │   └── IMessagingHealthProbe.cs
│   │   └── Students
│   │       └── StudentQueryResponse.cs
│   ├── BuildingBlocks.DatabaseMigration
│   │   ├── BuildingBlocks.DatabaseMigration.csproj
│   │   └── SqlMigrationRunner.cs
│   ├── BuildingBlocks.Http
│   │   ├── BuildingBlocks.Http.csproj
│   │   ├── CorrelationIdDelegatingHandler.cs
│   │   ├── ServiceQueryClientExtensions.cs
│   │   └── ServiceQueryOptions.cs
│   ├── BuildingBlocks.Messaging
│   │   ├── BuildingBlocks.Messaging.csproj
│   │   ├── ConsumerRegistrationExtensions.cs
│   │   ├── MassTransitMessagingHealthProbe.cs
│   │   ├── MessageEndpointNameFormatter.cs
│   │   ├── MessageTransport.cs
│   │   ├── MessagingOptions.cs
│   │   └── MessagingServiceCollectionExtensions.cs
│   ├── BuildingBlocks.Messaging.Abstractions
│   │   ├── BuildingBlocks.Messaging.Abstractions.csproj
│   │   └── MessagingContracts.cs
│   ├── BuildingBlocks.Messaging.IntegrationTests
│   │   ├── BuildingBlocks.Messaging.IntegrationTests.csproj
│   │   └── RabbitMqMessagingTests.cs
│   ├── BuildingBlocks.Presentation
│   │   ├── Api
│   │   │   └── ApiResponseFactory.cs
│   │   ├── BuildingBlocks.Presentation.csproj
│   │   ├── Cors
│   │   │   └── CorsOptions.cs
│   │   ├── Extensions
│   │   │   ├── ApplicationBuilderExtensions.cs
│   │   │   ├── CorsExtensions.cs
│   │   │   ├── EndpointRouteBuilderExtensions.cs
│   │   │   └── GatewaySwaggerExtensions.cs
│   │   ├── GlobalUsings.cs
│   │   └── Middleware
│   │       ├── ApiExceptionHandlingMiddleware.cs
│   │       └── CorrelationIdMiddleware.cs
│   ├── BuildingBlocks.Presentation.Tests
│   │   ├── BuildingBlocks.Presentation.Tests.csproj
│   │   ├── Endpoints
│   │   │   └── DatabaseHealthEndpointTests.cs
│   │   ├── Gateway
│   │   │   ├── GatewayCorsTests.cs
│   │   │   └── GatewaySwaggerDocumentTests.cs
│   │   └── Middleware
│   │       └── ApiMiddlewareTests.cs
│   └── BuildingBlocks.Shared
│       └── BuildingBlocks.Shared.csproj
├── Directory.Build.props
├── Dockerfile
├── Gateway
│   └── Lms.ApiGateway
│       ├── appsettings.Development.json
│       ├── appsettings.json
│       ├── Lms.ApiGateway.csproj
│       ├── Program.cs
│       └── Properties
│           └── launchSettings.json
├── Lms.sln
├── Services
│   ├── Course
│   │   ├── CourseService.Api
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.json
│   │   │   ├── Contracts
│   │   │   │   └── Courses
│   │   │   │       └── CourseListItemResponse.cs
│   │   │   ├── CourseService.Api.csproj
│   │   │   ├── Endpoints
│   │   │   │   └── Courses
│   │   │   │       ├── Export
│   │   │   │       │   └── ExportCoursesEndpoint.cs
│   │   │   │       ├── GetDetails
│   │   │   │       │   └── GetCourseDetailsEndpoint.cs
│   │   │   │       └── GetList
│   │   │   │           └── GetCoursesEndpoint.cs
│   │   │   ├── Mappers
│   │   │   │   └── CourseResponseMapper.cs
│   │   │   ├── Program.cs
│   │   │   └── Properties
│   │   │       └── launchSettings.json
│   │   ├── CourseService.Application
│   │   │   ├── Common
│   │   │   │   └── Errors
│   │   │   │       ├── CourseApplicationException.cs
│   │   │   │       ├── CourseErrorCodes.cs
│   │   │   │       └── CourseErrors.cs
│   │   │   ├── CourseService.Application.csproj
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── Repositories
│   │   │   │   ├── CourseListItemRecord.cs
│   │   │   │   ├── ICourseDetailsRepository.cs
│   │   │   │   └── ICourseListRepository.cs
│   │   │   └── UseCases
│   │   │       └── Courses
│   │   │           ├── Export
│   │   │           │   ├── CourseExportChunk.cs
│   │   │           │   ├── CourseExportPosition.cs
│   │   │           │   ├── CourseExportRow.cs
│   │   │           │   ├── CsvRowWriter.cs
│   │   │           │   ├── ExportCoursesHandler.cs
│   │   │           │   └── ExportCoursesQuery.cs
│   │   │           ├── GetDetails
│   │   │           │   ├── CourseDetailsResult.cs
│   │   │           │   └── GetCourseDetailsHandler.cs
│   │   │           └── GetList
│   │   │               ├── CourseSortField.cs
│   │   │               ├── GetCoursesHandler.cs
│   │   │               ├── GetCoursesQuery.cs
│   │   │               └── GetCoursesResult.cs
│   │   ├── CourseService.ComponentTests
│   │   │   ├── CourseService.ComponentTests.csproj
│   │   │   └── Endpoints
│   │   │       ├── ExportCoursesEndpointComponentTests.cs
│   │   │       └── GetCoursesEndpointComponentTests.cs
│   │   ├── CourseService.Domain
│   │   │   ├── Constants
│   │   │   │   └── CourseStatuses.cs
│   │   │   └── CourseService.Domain.csproj
│   │   ├── CourseService.Infrastructure
│   │   │   ├── CourseService.Infrastructure.csproj
│   │   │   ├── Database
│   │   │   │   └── Migrations
│   │   │   │       └── V001__create_learning_tables.sql
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── Health
│   │   │   │   └── CourseDatabaseHealthProbe.cs
│   │   │   └── Persistence
│   │   │       ├── CourseDbContext.cs
│   │   │       ├── Repositories
│   │   │       │   ├── EfCourseDetailsRepository.cs
│   │   │       │   └── EfCourseListRepository.cs
│   │   │       └── Scaffolded
│   │   │           ├── Course.cs
│   │   │           ├── Enrollment.cs
│   │   │           ├── Lesson.cs
│   │   │           └── LessonProgress.cs
│   │   └── CourseService.UnitTests
│   │       ├── Application
│   │       │   └── Courses
│   │       │       ├── Export
│   │       │       │   ├── CourseExportChunkTests.cs
│   │       │       │   ├── CsvRowWriterTests.cs
│   │       │       │   ├── ExportCoursesHandlerTests.cs
│   │       │       │   └── ExportCoursesQueryTests.cs
│   │       │       └── GetList
│   │       │           └── GetCoursesHandlerTests.cs
│   │       ├── CourseService.UnitTests.csproj
│   │       ├── Features
│   │       │   └── Courses
│   │       │       └── GetList
│   │       └── UnitTest1.cs
│   ├── Media
│   │   ├── MediaService.Api
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.json
│   │   │   ├── Contracts
│   │   │   │   ├── Media
│   │   │   │   │   ├── DirectUpload
│   │   │   │   │   │   ├── Complete
│   │   │   │   │   │   │   └── CompleteDirectUploadRequest.cs
│   │   │   │   │   │   └── CreateIntent
│   │   │   │   │   │       ├── CreateUploadIntentRequest.cs
│   │   │   │   │   │       └── CreateUploadIntentResponse.cs
│   │   │   │   │   └── Upload
│   │   │   │   │       ├── Requests
│   │   │   │   │       │   └── UploadMediaForm.cs
│   │   │   │   │       └── Responses
│   │   │   │   │           └── UploadMediaResponse.cs
│   │   │   │   ├── MediaDerivations
│   │   │   │   │   ├── GetThumbnailStatus
│   │   │   │   │   │   └── MediaThumbnailStatusResponse.cs
│   │   │   │   │   └── RetryThumbnail
│   │   │   │   │       └── RetryMediaThumbnailRequest.cs
│   │   │   │   ├── MediaUsageJobs
│   │   │   │   │   └── NotificationMediaUsageJobStatusResponse.cs
│   │   │   │   └── MediaUsages
│   │   │   │       ├── Create
│   │   │   │       │   ├── CreateMediaUsageRequest.cs
│   │   │   │       │   └── CreateMediaUsageResponse.cs
│   │   │   │       └── GetUrls
│   │   │   │           └── MediaUsageUrlResponse.cs
│   │   │   ├── Endpoints
│   │   │   │   ├── Health
│   │   │   │   │   └── MediaHealthEndpoint.cs
│   │   │   │   ├── Media
│   │   │   │   │   ├── DirectUpload
│   │   │   │   │   │   ├── Complete
│   │   │   │   │   │   │   └── CompleteDirectUploadEndpoint.cs
│   │   │   │   │   │   └── CreateIntent
│   │   │   │   │   │       └── CreateUploadIntentEndpoint.cs
│   │   │   │   │   ├── GetContent
│   │   │   │   │   │   └── GetMediaContentEndpoint.cs
│   │   │   │   │   └── Upload
│   │   │   │   │       ├── MediaRequestParser.cs
│   │   │   │   │       └── UploadMediaEndpoint.cs
│   │   │   │   ├── MediaDerivations
│   │   │   │   │   ├── GetThumbnailStatus
│   │   │   │   │   │   └── GetMediaThumbnailStatusEndpoint.cs
│   │   │   │   │   └── RetryThumbnail
│   │   │   │   │       └── RetryMediaThumbnailEndpoint.cs
│   │   │   │   ├── MediaUsageJobs
│   │   │   │   │   └── GetStatus
│   │   │   │   │       └── GetNotificationMediaUsageJobStatusEndpoint.cs
│   │   │   │   └── MediaUsages
│   │   │   │       ├── Create
│   │   │   │       │   └── CreateMediaUsageEndpoint.cs
│   │   │   │       ├── GetUrl
│   │   │   │       │   └── GetMediaUsageUrlEndpoint.cs
│   │   │   │       └── GetUrls
│   │   │   │           └── GetMediaUsageUrlsEndpoint.cs
│   │   │   ├── Mappers
│   │   │   │   └── MediaResponseMapper.cs
│   │   │   ├── MediaService.Api.csproj
│   │   │   ├── Program.cs
│   │   │   └── Properties
│   │   │       └── launchSettings.json
│   │   ├── MediaService.Application
│   │   │   ├── Common
│   │   │   │   └── Errors
│   │   │   │       ├── MediaApplicationException.cs
│   │   │   │       ├── MediaErrorCodes.cs
│   │   │   │       └── MediaErrors.cs
│   │   │   ├── Contracts
│   │   │   │   └── Messaging
│   │   │   │       └── GenerateMediaThumbnailV1.cs
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── Features
│   │   │   │   └── Media
│   │   │   │       ├── CompleteDirectUpload
│   │   │   │       └── CreateUploadIntent
│   │   │   ├── MediaService.Application.csproj
│   │   │   ├── Repositories
│   │   │   │   ├── CreateMediaUsageRecord.cs
│   │   │   │   ├── IMediaDerivationRepository.cs
│   │   │   │   ├── IMediaRepository.cs
│   │   │   │   ├── IMediaUploadFinalizer.cs
│   │   │   │   ├── IMediaUsageRepository.cs
│   │   │   │   ├── INotificationMediaUsageJobRepository.cs
│   │   │   │   ├── MediaRecord.cs
│   │   │   │   ├── MediaUsageOwnerQuery.cs
│   │   │   │   ├── MediaUsageRecord.cs
│   │   │   │   ├── MediaUsageUrlRecord.cs
│   │   │   │   ├── NotificationMediaUsageJobRecord.cs
│   │   │   │   └── PendingMediaRecord.cs
│   │   │   ├── Services
│   │   │   │   ├── Actors
│   │   │   │   │   ├── ActorValidationService.cs
│   │   │   │   │   ├── IActorValidationService.cs
│   │   │   │   │   ├── IActorValidator.cs
│   │   │   │   │   └── StudentActorValidator.cs
│   │   │   │   ├── Derivation
│   │   │   │   │   └── IThumbnailGenerator.cs
│   │   │   │   ├── Storage
│   │   │   │   │   ├── IStorage.cs
│   │   │   │   │   ├── IStorageHealthProbe.cs
│   │   │   │   │   ├── IStorageLocationAllocator.cs
│   │   │   │   │   ├── IStorageUploadPolicyProvider.cs
│   │   │   │   │   ├── StorageDownloadRequest.cs
│   │   │   │   │   ├── StorageHealthProbeResult.cs
│   │   │   │   │   ├── StorageMediaCategory.cs
│   │   │   │   │   ├── StorageObjectInfo.cs
│   │   │   │   │   ├── StorageObjectLocation.cs
│   │   │   │   │   ├── StorageObjectNotFoundException.cs
│   │   │   │   │   ├── StorageOperationException.cs
│   │   │   │   │   ├── StoragePromotionRequest.cs
│   │   │   │   │   ├── StorageUploadPolicy.cs
│   │   │   │   │   ├── StorageUploadPolicyRequest.cs
│   │   │   │   │   ├── StorageUploadRequest.cs
│   │   │   │   │   └── StorageValidationException.cs
│   │   │   │   ├── Students
│   │   │   │   │   └── IStudentLookup.cs
│   │   │   │   └── Urls
│   │   │   │       ├── IMediaUrlProvider.cs
│   │   │   │       └── MediaUrl.cs
│   │   │   └── UseCases
│   │   │       ├── Media
│   │   │       │   ├── DirectUpload
│   │   │       │   │   ├── Complete
│   │   │       │   │   │   ├── CompleteDirectUploadCommand.cs
│   │   │       │   │   │   └── CompleteDirectUploadHandler.cs
│   │   │       │   │   ├── CreateIntent
│   │   │       │   │   │   ├── CreateUploadIntentCommand.cs
│   │   │       │   │   │   ├── CreateUploadIntentHandler.cs
│   │   │       │   │   │   └── CreateUploadIntentResult.cs
│   │   │       │   │   ├── DirectUploadChecksum.cs
│   │   │       │   │   └── MediaContentTypeRules.cs
│   │   │       │   ├── GetContent
│   │   │       │   │   ├── GetMediaContentHandler.cs
│   │   │       │   │   ├── GetMediaContentQuery.cs
│   │   │       │   │   └── GetMediaContentResult.cs
│   │   │       │   └── Upload
│   │   │       │       ├── MediaUploadOptions.cs
│   │   │       │       ├── UploadMediaCommand.cs
│   │   │       │       ├── UploadMediaHandler.cs
│   │   │       │       └── UploadMediaResult.cs
│   │   │       ├── MediaDerivations
│   │   │       │   ├── GenerateThumbnail
│   │   │       │   │   ├── GenerateMediaThumbnailHandler.cs
│   │   │       │   │   ├── MediaThumbnailOptions.cs
│   │   │       │   │   └── MediaThumbnailPolicy.cs
│   │   │       │   ├── GetThumbnailStatus
│   │   │       │   │   └── GetMediaThumbnailStatusHandler.cs
│   │   │       │   └── RetryThumbnail
│   │   │       │       └── RetryMediaThumbnailHandler.cs
│   │   │       ├── MediaUsageJobs
│   │   │       │   ├── GetStatus
│   │   │       │   │   ├── GetNotificationMediaUsageJobStatusHandler.cs
│   │   │       │   │   └── NotificationMediaUsageJobStatusResult.cs
│   │   │       │   └── Process
│   │   │       │       └── NotificationMediaUsageJobLifecycleHandler.cs
│   │   │       └── MediaUsages
│   │   │           ├── Create
│   │   │           │   ├── CreateMediaUsageCommand.cs
│   │   │           │   ├── CreateMediaUsageHandler.cs
│   │   │           │   └── CreateMediaUsageResult.cs
│   │   │           ├── GetUrl
│   │   │           │   ├── GetMediaUsageUrlHandler.cs
│   │   │           │   └── GetMediaUsageUrlQuery.cs
│   │   │           ├── GetUrls
│   │   │           │   ├── GetMediaUsageUrlsHandler.cs
│   │   │           │   ├── GetMediaUsageUrlsQuery.cs
│   │   │           │   └── MediaUsageUrlResult.cs
│   │   │           └── RegisterNotification
│   │   │               └── RegisterNotificationMediaUsagesHandler.cs
│   │   ├── MediaService.ComponentTests
│   │   │   ├── Endpoints
│   │   │   │   ├── Media
│   │   │   │   │   └── DirectMediaUploadEndpointsComponentTests.cs
│   │   │   │   ├── MediaUsageJobs
│   │   │   │   │   └── GetNotificationMediaUsageJobStatusEndpointComponentTests.cs
│   │   │   │   └── MediaUsages
│   │   │   │       └── GetMediaUsageUrlEndpointsComponentTests.cs
│   │   │   └── MediaService.ComponentTests.csproj
│   │   ├── MediaService.Contracts
│   │   │   ├── MediaService.Contracts.csproj
│   │   │   └── Messaging
│   │   │       └── RegisterNotificationMediaUsageV1.cs
│   │   ├── MediaService.Domain
│   │   │   ├── Constants
│   │   │   │   ├── ActorTypes.cs
│   │   │   │   ├── MediaDerivation.cs
│   │   │   │   ├── MediaObjectStatuses.cs
│   │   │   │   ├── MediaOwnerServices.cs
│   │   │   │   ├── MediaOwnerTypes.cs
│   │   │   │   ├── MediaTypes.cs
│   │   │   │   ├── MediaUsageTypes.cs
│   │   │   │   └── NotificationMediaUsageJobStatuses.cs
│   │   │   ├── Entities
│   │   │   │   ├── Media.cs
│   │   │   │   └── MediaUsage.cs
│   │   │   ├── MediaService.Domain.csproj
│   │   │   └── ValueObjects
│   │   │       └── ActorReference.cs
│   │   ├── MediaService.Infrastructure
│   │   │   ├── Clients
│   │   │   │   └── Student
│   │   │   │       └── StudentLookupClient.cs
│   │   │   ├── Database
│   │   │   │   └── Migrations
│   │   │   │       ├── V001__create_media_tables.sql
│   │   │   │       ├── V002__add_media_upload_lifecycle_and_student_usage.sql
│   │   │   │       ├── V003__allow_reusing_soft_deleted_media_usages.sql
│   │   │   │       ├── V004__add_thumbnail_derivation_and_outbox.sql
│   │   │   │       ├── V005__add_media_draft_state.sql
│   │   │   │       └── V006__add_notification_media_usage_jobs.sql
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── Health
│   │   │   │   └── MediaDatabaseHealthProbe.cs
│   │   │   ├── MediaService.Infrastructure.csproj
│   │   │   ├── Persistence
│   │   │   │   ├── Context
│   │   │   │   │   ├── MediaDbContext.cs
│   │   │   │   │   └── MediaDbContext.Outbox.cs
│   │   │   │   ├── Mappers
│   │   │   │   │   ├── MediaDerivationPersistenceMapper.cs
│   │   │   │   │   ├── MediaPersistenceMapper.cs
│   │   │   │   │   └── MediaUsagePersistenceMapper.cs
│   │   │   │   ├── Repositories
│   │   │   │   │   ├── EfMediaDerivationRepository.cs
│   │   │   │   │   ├── EfMediaRepository.cs
│   │   │   │   │   ├── EfMediaUsageRepository.cs
│   │   │   │   │   └── EfNotificationMediaUsageJobRepository.cs
│   │   │   │   ├── Scaffolded
│   │   │   │   │   ├── MediaDerivationJob.cs
│   │   │   │   │   ├── MediaObject.cs
│   │   │   │   │   ├── MediaUsage.cs
│   │   │   │   │   └── NotificationMediaUsageJob.cs
│   │   │   │   └── Transactions
│   │   │   │       └── EfMediaUploadFinalizer.cs
│   │   │   ├── Services
│   │   │   │   ├── Thumbnail
│   │   │   │   │   ├── MediaThumbnailGenerator.cs
│   │   │   │   │   ├── NativeProcessRunner.cs
│   │   │   │   │   └── TemporaryMediaFileFactory.cs
│   │   │   │   └── Urls
│   │   │   │       └── ContentEndpointMediaUrlProvider.cs
│   │   │   └── Storage
│   │   │       └── Minio
│   │   │           ├── MinioClientRegistrations.cs
│   │   │           ├── MinioObjectKeyGenerator.cs
│   │   │           ├── MinioStorageLocationAllocator.cs
│   │   │           ├── MinioStorageOptions.cs
│   │   │           ├── MinioStorageOptionsValidator.cs
│   │   │           ├── MinioStorageRequestValidator.cs
│   │   │           ├── MinioStorageService.cs
│   │   │           ├── MinioUploadPolicyProvider.cs
│   │   │           ├── Sha256ReadStream.cs
│   │   │           └── ValidatedStorageUpload.cs
│   │   ├── MediaService.IntegrationTests
│   │   │   ├── Flows
│   │   │   │   └── MediaUploadUsageFlowTests.cs
│   │   │   ├── MediaService.IntegrationTests.csproj
│   │   │   └── Storage
│   │   │       └── MinioStorageServiceTests.cs
│   │   ├── MediaService.UnitTests
│   │   │   ├── Api
│   │   │   │   └── Endpoints
│   │   │   │       ├── Health
│   │   │   │       │   └── MediaHealthEndpointTests.cs
│   │   │   │       └── Media
│   │   │   │           ├── MediaCommandEndpointTests.cs
│   │   │   │           └── MediaRequestParserTests.cs
│   │   │   ├── Application
│   │   │   │   └── UseCases
│   │   │   │       ├── Media
│   │   │   │       │   ├── DirectUpload
│   │   │   │       │   │   ├── Complete
│   │   │   │       │   │   │   └── CompleteDirectUploadHandlerTests.cs
│   │   │   │       │   │   └── CreateIntent
│   │   │   │       │   │       └── CreateUploadIntentHandlerTests.cs
│   │   │   │       │   ├── GetContent
│   │   │   │       │   │   └── GetMediaContentHandlerTests.cs
│   │   │   │       │   └── Upload
│   │   │   │       │       └── UploadMediaHandlerTests.cs
│   │   │   │       ├── MediaUsageJobs
│   │   │   │       │   └── GetStatus
│   │   │   │       │       └── GetNotificationMediaUsageJobStatusHandlerTests.cs
│   │   │   │       └── MediaUsages
│   │   │   │           ├── Create
│   │   │   │           │   └── CreateMediaUsageHandlerTests.cs
│   │   │   │           ├── GetUrl
│   │   │   │           │   └── GetMediaUsageUrlHandlerTests.cs
│   │   │   │           ├── GetUrls
│   │   │   │           │   └── GetMediaUsageUrlsHandlerTests.cs
│   │   │   │           └── RegisterNotification
│   │   │   │               └── RegisterNotificationMediaUsagesHandlerTests.cs
│   │   │   ├── Architecture
│   │   │   │   ├── MediaLayerDependencyTests.cs
│   │   │   │   └── MediaSourceFileHeaderTests.cs
│   │   │   ├── Clients
│   │   │   │   └── StudentLookupClientTests.cs
│   │   │   ├── Derivation
│   │   │   │   └── MediaThumbnailGeneratorTests.cs
│   │   │   ├── Health
│   │   │   │   └── MediaDatabaseHealthProbeTests.cs
│   │   │   ├── MediaService.UnitTests.csproj
│   │   │   ├── Persistence
│   │   │   │   └── MediaPersistenceMapperTests.cs
│   │   │   ├── Storage
│   │   │   │   ├── MinioObjectKeyGeneratorTests.cs
│   │   │   │   ├── MinioStorageOptionsTests.cs
│   │   │   │   ├── MinioStorageRequestValidatorTests.cs
│   │   │   │   └── MinioUploadPolicyProviderTests.cs
│   │   │   └── TestDoubles
│   │   │       ├── StubActorValidationService.cs
│   │   │       ├── StubMediaRepository.cs
│   │   │       ├── StubNotificationMediaUsageJobRepository.cs
│   │   │       ├── StubStorage.cs
│   │   │       ├── StubStorageLocationAllocator.cs
│   │   │       └── StubStudentLookup.cs
│   │   └── MediaService.Worker
│   │       ├── Consumers
│   │       │   ├── MediaUsage
│   │       │   │   ├── RegisterNotificationMediaUsageConsumer.cs
│   │       │   │   └── RegisterNotificationMediaUsageFaultConsumer.cs
│   │       │   └── Thumbnail
│   │       │       ├── GenerateMediaThumbnailConsumer.cs
│   │       │       └── GenerateMediaThumbnailFaultConsumer.cs
│   │       ├── MediaService.Worker.csproj
│   │       └── Program.cs
│   ├── Notification
│   │   ├── NotificationService.Api
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.json
│   │   │   ├── Contracts
│   │   │   │   ├── NotificationBatches
│   │   │   │   │   ├── Requests
│   │   │   │   │   │   ├── CreateNotificationBatchRequest.cs
│   │   │   │   │   │   └── RetryFailedNotificationBatchRequest.cs
│   │   │   │   │   └── Responses
│   │   │   │   │       ├── NotificationBatchDeliveryStatusResponse.cs
│   │   │   │   │       ├── NotificationBatchFailedItemsResponse.cs
│   │   │   │   │       ├── NotificationBatchResponse.cs
│   │   │   │   │       └── NotificationBatchSnapshotStatusResponse.cs
│   │   │   │   └── Notifications
│   │   │   │       ├── Requests
│   │   │   │       │   └── CreateNotificationRequest.cs
│   │   │   │       └── Responses
│   │   │   │           └── NotificationResponse.cs
│   │   │   ├── Endpoints
│   │   │   │   ├── NotificationBatches
│   │   │   │   │   ├── Create
│   │   │   │   │   │   └── CreateNotificationBatchEndpoint.cs
│   │   │   │   │   ├── GetById
│   │   │   │   │   │   └── GetNotificationBatchByIdEndpoint.cs
│   │   │   │   │   ├── GetDeliveryStatus
│   │   │   │   │   │   └── GetNotificationBatchDeliveryStatusEndpoint.cs
│   │   │   │   │   ├── GetFailedItems
│   │   │   │   │   │   └── GetNotificationBatchFailedItemsEndpoint.cs
│   │   │   │   │   ├── GetList
│   │   │   │   │   │   └── GetNotificationBatchesEndpoint.cs
│   │   │   │   │   ├── GetSnapshotStatus
│   │   │   │   │   │   └── GetNotificationBatchSnapshotStatusEndpoint.cs
│   │   │   │   │   └── RetryFailed
│   │   │   │   │       └── RetryFailedNotificationBatchEndpoint.cs
│   │   │   │   └── Notifications
│   │   │   │       ├── Create
│   │   │   │       │   └── CreateNotificationEndpoint.cs
│   │   │   │       └── GetById
│   │   │   │           └── GetNotificationByIdEndpoint.cs
│   │   │   ├── Mappers
│   │   │   │   └── NotificationResponseMapper.cs
│   │   │   ├── NotificationService.Api.csproj
│   │   │   ├── Program.cs
│   │   │   └── Properties
│   │   │       └── launchSettings.json
│   │   ├── NotificationService.Application
│   │   │   ├── Common
│   │   │   │   └── Errors
│   │   │   │       ├── NotificationApplicationException.cs
│   │   │   │       └── NotificationErrors.cs
│   │   │   ├── Contracts
│   │   │   │   └── Messaging
│   │   │   │       ├── DispatchNotificationBatchV1.cs
│   │   │   │       └── SnapshotNotificationBatchV1.cs
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── NotificationService.Application.csproj
│   │   │   ├── Repositories
│   │   │   │   ├── INotificationBatchDispatchRepository.cs
│   │   │   │   ├── INotificationBatchRepository.cs
│   │   │   │   ├── INotificationRepository.cs
│   │   │   │   └── Models
│   │   │   │       ├── CreateNotificationBatchRecord.cs
│   │   │   │       ├── CreateNotificationRecord.cs
│   │   │   │       ├── NotificationBatchClaim.cs
│   │   │   │       ├── NotificationBatchContinuation.cs
│   │   │   │       ├── NotificationBatchDeliveryResult.cs
│   │   │   │       ├── NotificationBatchFailedItem.cs
│   │   │   │       ├── NotificationBatchFailedItemsPage.cs
│   │   │   │       ├── NotificationBatchListPage.cs
│   │   │   │       ├── NotificationBatchRetryCreation.cs
│   │   │   │       ├── NotificationBatchSnapshotProgress.cs
│   │   │   │       ├── NotificationBatchSummary.cs
│   │   │   │       ├── NotificationBatchWorkItem.cs
│   │   │   │       ├── NotificationSnapshotWork.cs
│   │   │   │       └── NotificationSummary.cs
│   │   │   ├── Services
│   │   │   │   ├── Content
│   │   │   │   │   └── NotificationMediaReferenceExtractor.cs
│   │   │   │   ├── Sending
│   │   │   │   │   └── INotificationSender.cs
│   │   │   │   └── Students
│   │   │   │       └── IStudentRecipientClient.cs
│   │   │   └── UseCases
│   │   │       ├── NotificationBatches
│   │   │       │   ├── Create
│   │   │       │   │   ├── CreateNotificationBatchCommand.cs
│   │   │       │   │   └── CreateNotificationBatchHandler.cs
│   │   │       │   ├── Dispatch
│   │   │       │   │   ├── DispatchNotificationBatchHandler.cs
│   │   │       │   │   └── NotificationBatchProcessingOptions.cs
│   │   │       │   ├── GetById
│   │   │       │   │   └── GetNotificationBatchByIdHandler.cs
│   │   │       │   ├── GetDeliveryStatus
│   │   │       │   │   ├── GetNotificationBatchDeliveryStatusHandler.cs
│   │   │       │   │   └── NotificationBatchDeliveryStatusResult.cs
│   │   │       │   ├── GetFailedItems
│   │   │       │   │   └── GetNotificationBatchFailedItemsHandler.cs
│   │   │       │   ├── GetList
│   │   │       │   │   └── GetNotificationBatchesHandler.cs
│   │   │       │   ├── GetSnapshotStatus
│   │   │       │   │   ├── GetNotificationBatchSnapshotStatusHandler.cs
│   │   │       │   │   └── NotificationBatchSnapshotStatusResult.cs
│   │   │       │   ├── NotificationBatchDuration.cs
│   │   │       │   ├── RetryFailed
│   │   │       │   │   └── RetryFailedNotificationBatchHandler.cs
│   │   │       │   └── Snapshot
│   │   │       │       └── SnapshotNotificationBatchHandler.cs
│   │   │       └── Notifications
│   │   │           ├── Create
│   │   │           │   ├── CreateNotificationCommand.cs
│   │   │           │   └── CreateNotificationHandler.cs
│   │   │           └── GetById
│   │   │               └── GetNotificationByIdHandler.cs
│   │   ├── NotificationService.ComponentTests
│   │   │   ├── Endpoints
│   │   │   │   ├── NotificationBatches
│   │   │   │   │   └── NotificationBatchEndpointsComponentTests.cs
│   │   │   │   └── Notifications
│   │   │   │       └── NotificationEndpointsComponentTests.cs
│   │   │   └── NotificationService.ComponentTests.csproj
│   │   ├── NotificationService.Domain
│   │   │   ├── Constants
│   │   │   │   └── NotificationTypes.cs
│   │   │   ├── Entities
│   │   │   │   ├── NotificationBatchItemState.cs
│   │   │   │   └── NotificationBatchState.cs
│   │   │   └── NotificationService.Domain.csproj
│   │   ├── NotificationService.Infrastructure
│   │   │   ├── Clients
│   │   │   │   └── Student
│   │   │   │       └── StudentRecipientClient.cs
│   │   │   ├── Database
│   │   │   │   └── Migrations
│   │   │   │       ├── V001__create_notification_tables.sql
│   │   │   │       ├── V002__add_snapshot_and_outbox.sql
│   │   │   │       ├── V003__add_batch_item_claim_lease.sql
│   │   │   │       └── V004__add_batch_management.sql
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── Health
│   │   │   │   └── NotificationDatabaseHealthProbe.cs
│   │   │   ├── NotificationService.Infrastructure.csproj
│   │   │   ├── Persistence
│   │   │   │   ├── Context
│   │   │   │   │   ├── NotificationDbContext.cs
│   │   │   │   │   └── NotificationDbContext.Outbox.cs
│   │   │   │   ├── Mappers
│   │   │   │   │   ├── NotificationBatchItemPersistenceMapper.cs
│   │   │   │   │   ├── NotificationBatchPersistenceMapper.cs
│   │   │   │   │   └── NotificationPersistenceMapper.cs
│   │   │   │   ├── Repositories
│   │   │   │   │   ├── EfNotificationBatchRepository.cs
│   │   │   │   │   └── EfNotificationRepository.cs
│   │   │   │   └── Scaffolded
│   │   │   │       ├── NotificationBatch.cs
│   │   │   │       ├── NotificationBatchItem.cs
│   │   │   │       └── Notification.cs
│   │   │   └── Services
│   │   │       └── Sending
│   │   │           ├── FakeNotificationSender.cs
│   │   │           └── SuccessfulNotificationSender.cs
│   │   ├── NotificationService.IntegrationTests
│   │   │   ├── NotificationService.IntegrationTests.csproj
│   │   │   └── Persistence
│   │   │       ├── NotificationBatchLeaseIntegrationTests.cs
│   │   │       └── NotificationBatchSnapshotIntegrationTests.cs
│   │   ├── NotificationService.UnitTests
│   │   │   ├── Application
│   │   │   │   ├── Services
│   │   │   │   │   └── Content
│   │   │   │   │       └── NotificationMediaReferenceExtractorTests.cs
│   │   │   │   └── UseCases
│   │   │   │       └── NotificationBatches
│   │   │   │           ├── Create
│   │   │   │           │   └── CreateNotificationBatchHandlerTests.cs
│   │   │   │           ├── Dispatch
│   │   │   │           │   └── DispatchNotificationBatchHandlerTests.cs
│   │   │   │           ├── GetDeliveryStatus
│   │   │   │           │   └── GetNotificationBatchDeliveryStatusHandlerTests.cs
│   │   │   │           ├── GetList
│   │   │   │           │   └── GetNotificationBatchesHandlerTests.cs
│   │   │   │           ├── GetSnapshotStatus
│   │   │   │           │   └── GetNotificationBatchSnapshotStatusHandlerTests.cs
│   │   │   │           ├── RetryFailed
│   │   │   │           │   └── RetryFailedNotificationBatchHandlerTests.cs
│   │   │   │           ├── Snapshot
│   │   │   │           │   └── SnapshotNotificationBatchHandlerTests.cs
│   │   │   │           └── TestDoubles
│   │   │   │               └── NotificationBatchTestDoubles.cs
│   │   │   ├── Architecture
│   │   │   │   ├── NotificationArchitectureTests.cs
│   │   │   │   └── NotificationSourceFileHeaderTests.cs
│   │   │   ├── Domain
│   │   │   │   └── Entities
│   │   │   │       └── NotificationBatchStateTests.cs
│   │   │   ├── Infrastructure
│   │   │   │   ├── NotificationInfrastructureDependencyInjectionTests.cs
│   │   │   │   └── Services
│   │   │   │       └── Sending
│   │   │   │           ├── FakeNotificationSenderTests.cs
│   │   │   │           └── SuccessfulNotificationSenderTests.cs
│   │   │   └── NotificationService.UnitTests.csproj
│   │   └── NotificationService.Worker
│   │       ├── Consumers
│   │       │   └── NotificationBatches
│   │       │       ├── DispatchNotificationBatchConsumer.cs
│   │       │       ├── SnapshotNotificationBatchConsumer.cs
│   │       │       └── SnapshotNotificationBatchConsumerDefinition.cs
│   │       ├── NotificationService.Worker.csproj
│   │       └── Program.cs
│   ├── Scheduler
│   │   ├── SchedulerService.Api
│   │   │   ├── appsettings.Development.json
│   │   │   ├── appsettings.json
│   │   │   ├── Program.cs
│   │   │   └── SchedulerService.Api.csproj
│   │   ├── SchedulerService.Application
│   │   │   └── SchedulerService.Application.csproj
│   │   ├── SchedulerService.Domain
│   │   │   └── SchedulerService.Domain.csproj
│   │   ├── SchedulerService.Infrastructure
│   │   │   ├── Database
│   │   │   │   └── Migrations
│   │   │   │       └── V001__create_scheduler_tables.sql
│   │   │   ├── Health
│   │   │   │   └── SchedulerDatabaseHealthProbe.cs
│   │   │   ├── Persistence
│   │   │   │   ├── Scaffolded
│   │   │   │   │   ├── BackgroundJob.cs
│   │   │   │   │   └── BackgroundJobRun.cs
│   │   │   │   └── SchedulerDbContext.cs
│   │   │   └── SchedulerService.Infrastructure.csproj
│   │   ├── SchedulerService.UnitTests
│   │   │   ├── Health
│   │   │   │   └── SchedulerDatabaseHealthProbeTests.cs
│   │   │   └── SchedulerService.UnitTests.csproj
│   │   └── SchedulerService.Worker
│   │       ├── Program.cs
│   │       └── SchedulerService.Worker.csproj
│   └── Student
│       ├── StudentService.Api
│       │   ├── appsettings.Development.json
│       │   ├── appsettings.json
│       │   ├── Endpoints
│       │   │   ├── GetStudentsEndpoint.cs
│       │   │   └── StudentEndpoints.cs
│       │   ├── Program.cs
│       │   ├── Properties
│       │   │   └── launchSettings.json
│       │   └── StudentService.Api.csproj
│       ├── StudentService.Application
│       │   ├── DependencyInjection.cs
│       │   ├── Features
│       │   │   └── Students
│       │   │       ├── GetById
│       │   │       │   ├── GetStudentByIdHandler.cs
│       │   │       │   ├── IStudentRepository.cs
│       │   │       │   ├── StudentApplicationException.cs
│       │   │       │   ├── StudentErrorCodes.cs
│       │   │       │   └── StudentErrorMessages.cs
│       │   │       └── GetList
│       │   │           ├── GetStudentsHandler.cs
│       │   │           ├── GetStudentsQuery.cs
│       │   │           ├── GetStudentsResult.cs
│       │   │           ├── IStudentListRepository.cs
│       │   │           ├── StudentListItem.cs
│       │   │           ├── StudentListValidationException.cs
│       │   │           └── StudentSortField.cs
│       │   └── StudentService.Application.csproj
│       ├── StudentService.ComponentTests
│       │   ├── Endpoints
│       │   │   └── GetStudentsEndpointComponentTests.cs
│       │   └── StudentService.ComponentTests.csproj
│       ├── StudentService.Domain
│       │   └── StudentService.Domain.csproj
│       ├── StudentService.Infrastructure
│       │   ├── Database
│       │   │   └── Migrations
│       │   │       └── V001__create_students_table.sql
│       │   ├── DependencyInjection.cs
│       │   ├── Health
│       │   │   └── StudentDatabaseHealthProbe.cs
│       │   ├── Persistence
│       │   │   ├── EfStudentRepository.cs
│       │   │   ├── Scaffolded
│       │   │   │   └── Student.cs
│       │   │   └── StudentDbContext.cs
│       │   └── StudentService.Infrastructure.csproj
│       ├── StudentService.IntegrationTests
│       │   ├── Persistence
│       │   │   └── StudentListRepositoryIntegrationTests.cs
│       │   └── StudentService.IntegrationTests.csproj
│       └── StudentService.UnitTests
│           ├── ApiRoutesTests.cs
│           ├── GetStudentByIdHandlerTests.cs
│           ├── GetStudentsHandlerTests.cs
│           ├── GetStudentsQueryTests.cs
│           ├── StudentEndpointTests.cs
│           └── StudentService.UnitTests.csproj
└── Tools
    ├── Lms.DataSeeder
    │   ├── DeterministicSeedData.cs
    │   ├── Lms.DataSeeder.csproj
    │   ├── MySqlSeedRunner.cs
    │   ├── Program.cs
    │   ├── SeedOptions.cs
    │   └── SeedProgress.cs
    ├── Lms.DataSeeder.IntegrationTests
    │   ├── Lms.DataSeeder.IntegrationTests.csproj
    │   └── MySqlSeedRunnerTests.cs
    └── Lms.DataSeeder.UnitTests
        ├── DeterministicSeedDataTests.cs
        ├── Lms.DataSeeder.UnitTests.csproj
        └── SeedOptionsTests.cs

329 directories, 483 files