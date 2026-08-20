ALTER TABLE media_usages
    DROP CHECK chk_media_usages_owner_type,
    ADD CONSTRAINT chk_media_usages_owner_type
        CHECK (owner_type IN (
            'COURSE_THUMBNAIL',
            'COURSE_GALLERY',
            'COURSE_DESCRIPTION',
            'LESSON_CONTENT',
            'LESSON_ATTACHMENT',
            'NOTIFICATION_BODY',
            'STUDENT_AVATAR',
            'MEDIA_THUMBNAIL'
        ));
