-- Insert test users
INSERT INTO users (id, username, password_hash, role)
VALUES 
    ('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'testartist', 'hash_here', 'Artist'),
    ('b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a12', 'testuser', 'hash_here', 'Regular')
ON CONFLICT (username) DO NOTHING;

-- Insert some genres
INSERT INTO genres (id, name)
VALUES 
    ('c0eebc99-9c0b-4ef8-bb6d-6bb9bd380a13', 'Rock'),
    ('d0eebc99-9c0b-4ef8-bb6d-6bb9bd380a14', 'Jazz'),
    ('e0eebc99-9c0b-4ef8-bb6d-6bb9bd380a15', 'Classical')
ON CONFLICT (name) DO NOTHING;

-- Insert test tracks
DO $$
DECLARE
    v_track_id UUID;
BEGIN
    CALL sp_add_track(
        'Test Track 1',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        180.5,
        '/files/track1.mp3',
        ARRAY['Rock', 'Jazz'],
        v_track_id
    );

    CALL sp_add_track(
        'Test Track 2',
        'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
        240.0,
        '/files/track2.mp3',
        ARRAY['Classical'],
        v_track_id
    );
END $$;
