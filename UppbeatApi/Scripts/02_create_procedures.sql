-- Add Track procedure
CREATE OR REPLACE PROCEDURE sp_add_track(
    p_name VARCHAR(200),
    p_artist_id UUID,
    p_duration DOUBLE PRECISION,
    p_file TEXT,
    p_genres TEXT[],
    INOUT p_track_id UUID DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Insert track
    INSERT INTO tracks (name, artist_id, duration, file)
    VALUES (p_name, p_artist_id, p_duration, p_file)
    RETURNING id INTO p_track_id;

    -- Insert genres if they don't exist and create track-genre relationships
    WITH new_genres AS (
        INSERT INTO genres (name)
        SELECT DISTINCT unnest(p_genres)
        ON CONFLICT (name) DO NOTHING
        RETURNING id, name
    ),
    all_genres AS (
        SELECT id, name FROM new_genres
        UNION ALL
        SELECT id, name FROM genres WHERE name = ANY(p_genres)
    )
    INSERT INTO track_genres (track_id, genre_id)
    SELECT p_track_id, id FROM all_genres;
END;
$$;

-- Get Track by ID procedure
CREATE OR REPLACE FUNCTION sp_get_track_by_id(p_track_id UUID)
RETURNS TABLE (
    "Id" UUID,
    "Name" VARCHAR(200),
    "ArtistId" UUID,
    "Duration" DOUBLE PRECISION,
    "File" TEXT,
    "Genres" TEXT[],
    "CreatedAt" TIMESTAMP WITH TIME ZONE,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "Artist.Id" UUID,
    "Artist.Name" VARCHAR(100),
    "Artist.Role" VARCHAR(50)
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        t.id AS "Id",
        t.name AS "Name",
        t.artist_id AS "ArtistId",
        t.duration AS "Duration",
        t.file AS "File",
        COALESCE(ARRAY_AGG(g.name) FILTER (WHERE g.name IS NOT NULL), ARRAY[]::VARCHAR[])::TEXT[] AS "Genres",
        t.created_at AS "CreatedAt",
        t.updated_at AS "UpdatedAt",
        u.id AS "Artist.Id",
        u.username AS "Artist.Name",
        u.role::VARCHAR AS "Artist.Role"
    FROM tracks t
    JOIN users u ON t.artist_id = u.id
    LEFT JOIN track_genres tg ON t.id = tg.track_id
    LEFT JOIN genres g ON tg.genre_id = g.id
    WHERE t.id = p_track_id
    GROUP BY t.id, t.name, t.artist_id, t.duration, t.file, t.created_at, t.updated_at, u.id, u.username, u.role;
END;
$$;

-- Get Tracks with filtering and pagination
CREATE OR REPLACE FUNCTION sp_get_tracks(
    p_genre VARCHAR(50) DEFAULT NULL,
    p_search VARCHAR DEFAULT NULL,
    p_page INTEGER DEFAULT 1,
    p_page_size INTEGER DEFAULT 10
)
RETURNS TABLE (
    "Id" UUID,
    "Name" VARCHAR(200),
    "ArtistId" UUID,
    "Duration" DOUBLE PRECISION,
    "File" TEXT,
    "Genres" TEXT[],
    "CreatedAt" TIMESTAMP WITH TIME ZONE,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "Artist.Id" UUID,
    "Artist.Name" VARCHAR(100),
    "Artist.Role" VARCHAR(50)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_offset INTEGER;
BEGIN
    -- Calculate pagination offset
    v_offset := (p_page - 1) * p_page_size;

    -- Return query
    RETURN QUERY
    WITH filtered_tracks AS (
        SELECT DISTINCT t.id
        FROM tracks t
        JOIN users u ON t.artist_id = u.id
        LEFT JOIN track_genres tg ON t.id = tg.track_id
        LEFT JOIN genres g ON tg.genre_id = g.id
        WHERE 
            (p_genre IS NULL OR g.name = p_genre)
            AND (
                p_search IS NULL 
                OR t.name ILIKE '%' || p_search || '%'
                OR u.username ILIKE '%' || p_search || '%'
            )
    )
    SELECT 
        t.id AS "Id",
        t.name AS "Name",
        t.artist_id AS "ArtistId",
        t.duration AS "Duration",
        t.file AS "File",
        COALESCE(ARRAY_AGG(g.name) FILTER (WHERE g.name IS NOT NULL), ARRAY[]::VARCHAR[])::TEXT[] AS "Genres",
        t.created_at AS "CreatedAt",
        t.updated_at AS "UpdatedAt",
        u.id AS "Artist.Id",
        u.username AS "Artist.Name",
        u.role::VARCHAR AS "Artist.Role"
    FROM tracks t
    JOIN users u ON t.artist_id = u.id
    LEFT JOIN track_genres tg ON t.id = tg.track_id
    LEFT JOIN genres g ON tg.genre_id = g.id
    WHERE t.id IN (SELECT id FROM filtered_tracks)
    GROUP BY t.id, t.name, t.artist_id, t.duration, t.file, t.created_at, t.updated_at, u.id, u.username, u.role
    ORDER BY t.created_at DESC
    LIMIT p_page_size
    OFFSET v_offset;
END;
$$;

-- Update Track procedure
CREATE OR REPLACE PROCEDURE sp_update_track(
    p_track_id UUID,
    p_name VARCHAR(200),
    p_duration DOUBLE PRECISION,
    p_file TEXT,
    p_genres TEXT[]
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Update track
    UPDATE tracks 
    SET 
        name = p_name,
        duration = p_duration,
        file = p_file,
        updated_at = CURRENT_TIMESTAMP
    WHERE id = p_track_id;

    -- Delete existing genre relationships
    DELETE FROM track_genres WHERE track_id = p_track_id;

    -- Insert new genres if they don't exist and create track-genre relationships
    WITH new_genres AS (
        INSERT INTO genres (name)
        SELECT DISTINCT unnest(p_genres)
        ON CONFLICT (name) DO NOTHING
        RETURNING id, name
    ),
    all_genres AS (
        SELECT id, name FROM new_genres
        UNION ALL
        SELECT id, name FROM genres WHERE name = ANY(p_genres)
    )
    INSERT INTO track_genres (track_id, genre_id)
    SELECT p_track_id, id FROM all_genres;
END;
$$;

-- Delete Track procedure
CREATE OR REPLACE PROCEDURE sp_delete_track(
    p_track_id UUID
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM tracks WHERE id = p_track_id;
END;
$$;

-- Get User by ID procedure
CREATE OR REPLACE FUNCTION sp_get_user_by_id(p_user_id UUID)
RETURNS TABLE (
    "Id" UUID,
    "Name" VARCHAR(100),
    "Role" VARCHAR(50)
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT 
        u.id AS "Id",
        u.username AS "Name",
        u.role::VARCHAR AS "Role"
    FROM users u
    WHERE u.id = p_user_id;
END;
$$;
