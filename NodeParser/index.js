const spawn = require("child_process").spawn;
const fs = require("fs");
const path = require("path");
const https = require('https');

//Riot Client
async function updateRiotClientManifest() {
    https.get("https://clientconfig.rpg.riotgames.com/api/v1/config/public?version=99.0.0.9999999&patchline=KeystoneFoundationLiveWin&app=Riot%20Client&namespace=keystone.self_update", async function(config) {
        config.setEncoding("utf8");
        let data = "";
        config.on("data", async (response) => {
            data += response;
        });
        config.on("end", async () => {
            data = JSON.parse(data);
            let item = data["keystone.self_update.manifest_url"];
            if (!fs.existsSync(`./${item.split("/").pop()}`)) {
                const file = fs.createWriteStream(item.split("/").pop());
                https.get(item, function(res) {
                    res.pipe(file);
                    file.on("finish", () => {
                        file.close();
                        fs.renameSync(`./${item.split("/").pop()}`, './ritoclient.manifest');
                        console.log("Riot Client manifest has been successfully updated!");
                        parseRiotClientManifest(item.split("/").pop());
                    });
                });
            }
        });
    })
}
async function parseRiotClientManifest(manifest) {
    const downloader = spawn("./tools/ManifestDownloader.exe", ["./ritoclient.manifest", "--print-manifest"]);
    downloader.on("exit", async (data) => {
        fs.renameSync(`./${manifest.replace(".manifest", ".json")}`, './ritoclient.json')
        let json = fs.readFileSync("./ritoclient.json");
        json = JSON.parse(json);
        console.log("Riot Client manifest has been successfully parsed!");
        updateRiotClient(json["files"].pop()["path"], "./ritoclient.manifest")
    })
}
async function updateRiotClient(lastFile, manifest) {
    let finished = false;
    const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "-b", "https://ks-foundation.secure.dyn.riotcdn.net/channels/public/bundles", "-t", "8", "-o", "./Game/Riot Client"]);
    downloader.stdout.on('data', async (data) => {
        if (data.toString("utf8").includes(lastFile)) {
            if (!finished) {
                finished = true;
                console.log("Riot Client has been successfully updated!");
            }
        } else {
            console.log(data.toString("utf8"));
        }
    });
    downloader.on("exit", async (data) => {
        if (!finished) {
            updateRiotClient(lastFile, manifest);
        } else {
            updateManifest();
        }
    })
}

//VALORANT
async function updateGame(lastFile, manifest) {
    let finished = false;
    const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "-b", "https://valorant.secure.dyn.riotcdn.net/channels/public/bundles", "-t", "8", "-o", "./Game/VALORANT"]);
    downloader.stdout.on('data', async (data) => {
        if (data.toString("utf8").includes(lastFile)) {
            if (!finished) {
                finished = true;
                console.log("VALORANT has been successfully updated!");
                parseAssets();
            }
        } else {
            console.log(data.toString("utf8"));
        }
    });
    downloader.on("exit", async (data) => {
        if (!finished) {
            updateGame(lastFile, manifest);
        }
    })
}
async function parseAssets() {
    const parser = spawn("./tools/ValoParser.exe", ["./Game/VALORANT"]);
    parser.stdout.on('data', async (data) => {
        console.log(`${data.toString("utf8").replaceLast("\n", "")}`);
    });
    parser.on("exit", async (data) => {
        fs.rename(`./manifest.json`, './files/manifest.json', function(err) {
            if (!err) {
                parseVersion();
            }
        });
    })
}
async function parseManifest(manifest) {
    const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "--print-manifest"]);
    downloader.on("exit", async (data) => {
        fs.readFile(path.resolve(__dirname, manifest.replace(".manifest", ".json")),"utf-8", function(err, data) {
            if(!err) {
                fs.rename(`./${manifest.replace(".manifest", ".json")}`, './Game/VALORANT.json', function(err) {
                    if (!err) {
                        let json = JSON.parse(data);
                        console.log("VALORANT manifest has been successfully parsed!");
                        updateGame(json.files[json.files.length - 1].path, manifest);
                    }
                });
            }
        })
    })
}
async function updateManifest() {
    https.get("https://clientconfig.rpg.riotgames.com/api/v1/config/public?namespace=keystone.products.valorant.patchlines", async function(config) {
        config.setEncoding("utf8");
        let data = "";
        config.on("data", async (response) => {
            data += response;
        });
        config.on("end", async () => {
            data = JSON.parse(data);
            for (let i = 0; i < data["keystone.products.valorant.patchlines.live"]["platforms"]["win"]["configurations"].length; i++) {
                let item = data["keystone.products.valorant.patchlines.live"]["platforms"]["win"]["configurations"][i];
                if (item["id"] == "na") {
                    if (!fs.existsSync(`./${item["patch_url"].split("/").pop()}`)) {
                        const file = fs.createWriteStream(item["patch_url"].split("/").pop());
                        https.get(item["patch_url"], function(res) {
                            res.pipe(file);
                            file.on("finish", () => {
                                file.close();
                                console.log("VALORANT manifest has been successfully updated!");
                                parseManifest(item["patch_url"].split("/").pop());
                            });
                        });
                    }
                }
            }
        });
    })
}

//Overall
async function parseVersion() {
    const downloader = spawn("python ./tools/VersionParser.py", ["./Game"]);
    downloader.stdout.on('data', async (data) => {
        let json = JSON.parse(data.toString("utf8"));
        fs.writeFileSync("./files/version.json", JSON.stringify(json));
        console.log("API data has been successfully updated!");
    });
}

updateRiotClientManifest();