-- API: POST media usage reorder | ReorderAsync
START TRANSACTION;
SELECT u.* FROM media_usages u WHERE u.owner_service=@ownerService AND u.owner_type=@ownerType AND u.owner_id=@ownerId AND u.deleted_at IS NULL;
UPDATE media_usages SET display_order=@displayOrder1 WHERE id=@usageId1;
UPDATE media_usages SET display_order=@displayOrder2 WHERE id=@usageId2;
COMMIT;
