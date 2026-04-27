// Place in: %appdata%\spicetify\Extensions\lyric-bridge.js
(async function () {
    while (!Spicetify?.Player?.addEventListener || !Spicetify?.Player?.data) {
        await new Promise(resolve => setTimeout(resolve, 100));
    }

    console.log("[lyric-bridge] Ready, attaching listeners...");

    const PORT = 9513;
    const ENDPOINT = `http://localhost:${PORT}/track`;
    const COMMAND_URL = `http://localhost:${PORT}/command/poll`;
    const COMMAND_POLL_INTERVAL = 5;

    function getPayload() {
        try {
            const data = Spicetify.Player.data;
            if (!data?.item) return null;

            return {
                title: data.item.name ?? "",
                artist: data.item.artists?.[0]?.name ?? "",
                duration: (data.duration ?? 0) / 1000,
                position: Spicetify.Player.getProgress() / 1000,
                isPlaying: !data.isPaused,
                albumArt: data.item.album?.images?.[0]?.url ?? ""
            };
        } catch (e) {
            console.error("[lyric-bridge] getPayload error:", e);
            return null;
        }
    }

    function post(payload) {
        if (!payload) return;

        try {
            const xhr = new XMLHttpRequest();
            xhr.open("POST", ENDPOINT, true);
            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.onerror = () =>
                console.warn("[lyric-bridge] POST failed — is the C# listener running?");
            xhr.send(JSON.stringify(payload));
        } catch (e) {
            console.error("[lyric-bridge] post error:", e);
        }
    }

    async function pollCommands() {
        while (true) {
            try {
                const res = await fetch(COMMAND_URL);
                if (res.ok) {
                    const data = await res.json();

                    if (data?.command) {
                        console.log("[lyric-bridge] Command received:", data.command);

                        switch (data.command) {
                            case "next":
                                Spicetify.Player.next();
                                break;

                            case "previous":
                                Spicetify.Player.back();
                                break;

                            case "playpause":
                                Spicetify.Player.togglePlay();
                                break;

                            case "volumeup":
                                Spicetify.Player.setVolume(
                                    Math.min(1, Spicetify.Player.getVolume() + 0.1)
                                );
                                break;

                            case "volumedown":
                                Spicetify.Player.setVolume(
                                    Math.max(0, Spicetify.Player.getVolume() - 0.1)
                                );
                                break;
                        }
                    }
                }
            } catch (e) {}

            await new Promise(resolve =>
                setTimeout(resolve, COMMAND_POLL_INTERVAL)
            );
        }
    }

    Spicetify.Player.addEventListener("songchange", () => {
        console.log("[lyric-bridge] songchange fired");
        post(getPayload());
    });

    Spicetify.Player.addEventListener("onplaypause", () => post(getPayload()));
    Spicetify.Player.addEventListener("onprogress", () => post(getPayload()));

    const initial = getPayload();
    console.log("[lyric-bridge] Initial payload:", initial);
    post(initial);

    pollCommands();
})();