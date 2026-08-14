ALTER TABLE notification_batches
    DROP CHECK chk_notification_batches_status,
    MODIFY COLUMN status VARCHAR(20) CHARACTER SET ascii NOT NULL
        COMMENT 'PENDING | SNAPSHOTTING | SNAPSHOT_READY | PROCESSING | COMPLETED | PARTIAL_FAILED | FAILED',
    ADD CONSTRAINT chk_notification_batches_status
        CHECK (status IN ('PENDING', 'SNAPSHOTTING', 'SNAPSHOT_READY', 'PROCESSING', 'COMPLETED', 'PARTIAL_FAILED', 'FAILED'));

CREATE TABLE InboxState (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    MessageId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    ConsumerId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    LockId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    RowVersion BINARY(8) NULL,
    Received DATETIME(6) NOT NULL,
    ReceiveCount INT NOT NULL,
    ExpirationTime DATETIME(6) NULL,
    Consumed DATETIME(6) NULL,
    Delivered DATETIME(6) NULL,
    LastSequenceNumber BIGINT NULL,
    CONSTRAINT PK_InboxState PRIMARY KEY (Id),
    CONSTRAINT AK_InboxState_MessageId_ConsumerId UNIQUE (MessageId, ConsumerId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE OutboxState (
    OutboxId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    LockId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    RowVersion BINARY(8) NULL,
    Created DATETIME(6) NOT NULL,
    Delivered DATETIME(6) NULL,
    LastSequenceNumber BIGINT NULL,
    CONSTRAINT PK_OutboxState PRIMARY KEY (OutboxId),
    INDEX IX_OutboxState_Created (Created)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE OutboxMessage (
    SequenceNumber BIGINT NOT NULL AUTO_INCREMENT,
    EnqueueTime DATETIME(6) NULL,
    SentTime DATETIME(6) NOT NULL,
    Headers LONGTEXT NULL,
    Properties LONGTEXT NULL,
    InboxMessageId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    InboxConsumerId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    OutboxId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    MessageId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    ContentType VARCHAR(256) NOT NULL,
    MessageType LONGTEXT NOT NULL,
    Body LONGTEXT NOT NULL,
    ConversationId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    CorrelationId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    InitiatorId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    RequestId CHAR(36) CHARACTER SET ascii COLLATE ascii_bin NULL,
    SourceAddress VARCHAR(256) NULL,
    DestinationAddress VARCHAR(256) NULL,
    ResponseAddress VARCHAR(256) NULL,
    FaultAddress VARCHAR(256) NULL,
    ExpirationTime DATETIME(6) NULL,
    CONSTRAINT PK_OutboxMessage PRIMARY KEY (SequenceNumber),
    INDEX IX_OutboxMessage_EnqueueTime (EnqueueTime),
    INDEX IX_OutboxMessage_ExpirationTime (ExpirationTime),
    INDEX IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber (
        InboxMessageId,
        InboxConsumerId,
        SequenceNumber
    ),
    INDEX IX_OutboxMessage_OutboxId_SequenceNumber (OutboxId, SequenceNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
