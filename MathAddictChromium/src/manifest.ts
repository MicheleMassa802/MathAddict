export default {
    manifest_version: 3,
    name: "Math Addict Injector",
    version: "0.0.2",
    description: "A basic demo of what MathAddict will be able to do through a Unity WebGL Game!",
    permissions: ["tabs", "scripting"],
    web_accessible_resources: [
        {
            resources: ["GameBuild/**", "GameBuild/**/*"],
            matches: ["<all_urls>"],
        },
    ],

    action: {
        default_popup: "popup.html",
    },

    icons: {
        "32": "WaveTempIcon.png"
    },

    content_security_policy: {
        "extension_pages": "script-src 'self' 'wasm-unsafe-eval'; object-src 'self'"
    },
    host_permissions: ["<all_urls>"]
};