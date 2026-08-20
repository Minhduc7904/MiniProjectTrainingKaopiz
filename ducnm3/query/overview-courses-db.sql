SET SESSION group_concat_max_len = 1000000;

SELECT
    t.TABLE_NAME AS table_name,

    CASE t.TABLE_NAME
        WHEN 'courses' THEN (SELECT COUNT(*) FROM lms_course_db.courses)
        WHEN 'lessons' THEN (SELECT COUNT(*) FROM lms_course_db.lessons)
        WHEN 'enrollments' THEN (SELECT COUNT(*) FROM lms_course_db.enrollments)
        WHEN 'lesson_progresses' THEN (SELECT COUNT(*) FROM lms_course_db.lesson_progresses)
    END AS exact_row_count,

    t.TABLE_ROWS AS estimated_row_count,

    ROUND(t.DATA_LENGTH / 1024 / 1024, 2) AS data_mb,
    ROUND(t.INDEX_LENGTH / 1024 / 1024, 2) AS index_mb,
    ROUND(
        (t.DATA_LENGTH + t.INDEX_LENGTH) / 1024 / 1024,
        2
    ) AS total_mb,

    t.ENGINE AS engine,

    (
        SELECT GROUP_CONCAT(
            CONCAT(
                c.ORDINAL_POSITION,
                '. ',
                c.COLUMN_NAME,
                ' ',
                c.COLUMN_TYPE,
                ' | NULL=', c.IS_NULLABLE,
                ' | DEFAULT=', COALESCE(CAST(c.COLUMN_DEFAULT AS CHAR), 'NULL'),
                ' | KEY=', COALESCE(c.COLUMN_KEY, ''),
                ' | EXTRA=', COALESCE(c.EXTRA, '')
            )
            ORDER BY c.ORDINAL_POSITION
            SEPARATOR '\n'
        )
        FROM information_schema.COLUMNS c
        WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA
          AND c.TABLE_NAME = t.TABLE_NAME
    ) AS columns_structure,

    (
        SELECT GROUP_CONCAT(
            CONCAT(
                'INDEX=', s.INDEX_NAME,
                ' | COLUMN=', s.COLUMN_NAME,
                ' | SEQ=', s.SEQ_IN_INDEX,
                ' | UNIQUE=', IF(s.NON_UNIQUE = 0, 'YES', 'NO'),
                ' | TYPE=', s.INDEX_TYPE,
                ' | CARDINALITY=', COALESCE(CAST(s.CARDINALITY AS CHAR), 'NULL')
            )
            ORDER BY s.INDEX_NAME, s.SEQ_IN_INDEX
            SEPARATOR '\n'
        )
        FROM information_schema.STATISTICS s
        WHERE s.TABLE_SCHEMA = t.TABLE_SCHEMA
          AND s.TABLE_NAME = t.TABLE_NAME
    ) AS indexes,

    (
        SELECT GROUP_CONCAT(
            CONCAT(
                'CONSTRAINT=', k.CONSTRAINT_NAME,
                ' | COLUMN=', k.COLUMN_NAME,
                ' -> ',
                k.REFERENCED_TABLE_NAME,
                '.',
                k.REFERENCED_COLUMN_NAME
            )
            ORDER BY k.CONSTRAINT_NAME, k.ORDINAL_POSITION
            SEPARATOR '\n'
        )
        FROM information_schema.KEY_COLUMN_USAGE k
        WHERE k.TABLE_SCHEMA = t.TABLE_SCHEMA
          AND k.TABLE_NAME = t.TABLE_NAME
          AND k.REFERENCED_TABLE_NAME IS NOT NULL
    ) AS foreign_keys

FROM information_schema.TABLES t

WHERE t.TABLE_SCHEMA = 'lms_course_db'
  AND t.TABLE_NAME IN (
      'courses',
      'lessons',
      'enrollments',
      'lesson_progresses'
  )

ORDER BY t.TABLE_NAME;