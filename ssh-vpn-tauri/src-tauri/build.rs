fn main() {
    if std::env::var("TARGET").map_or(false, |t| t.contains("windows")) {
        let mut res = winres::WindowsResource::new();
        res.set_manifest_file("src/app.manifest");
        res.compile().unwrap();
    }
    tauri_build::build()
}
