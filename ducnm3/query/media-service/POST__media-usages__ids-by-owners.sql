-- API: POST ids-by-owners | GetActiveUsageIdsByOwnersAsync loops once per distinct owner.
SELECT u.id FROM media_usages u WHERE u.deleted_at IS NULL AND u.owner_service=@ownerService AND u.owner_type=@ownerType AND u.owner_id=@ownerId;
