const spawn = require("child_process").spawn;
const exec = require("child_process").exec;
const fs = require("fs");
const path = require("path");
const https = require('https');
const dataConfig = require("./config.json");

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
                        console.log("Riot Client manifest has been successfully updated!");
                        parseRiotClientManifest(item.split("/").pop());
                    });
                });
            }
        });
    })
}
async function parseRiotClientManifest(manifest) {
    const downloader = spawn("./tools/ManifestDownloader.exe", [`./${manifest}`, "--print-manifest"]);
    downloader.on("exit", async (data) => {
        fs.renameSync(`./${manifest.replace(".manifest", ".json")}`, './RiotClient.json')
        let json = fs.readFileSync("./RiotClient.json");
        json = JSON.parse(json);
        console.log("Riot Client manifest has been successfully parsed!");
        updateRiotClient(json["files"].pop()["path"], `./${manifest}`)
    })
}
async function updateRiotClient(lastFile, manifest) {
    let finished = false;
    const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "-b", "https://ks-foundation.secure.dyn.riotcdn.net/channels/public/bundles", "-t", "8", "-o", `${dataConfig.path.RiotClient}`]);
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
        }
    })
}

//VALORANT
async function updateGame(lastFile, manifest) {
    let finished = false;
    const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "-b", "https://valorant.secure.dyn.riotcdn.net/channels/public/bundles", "-t", "8", "-o", `${dataConfig.path.VALORANT}`]);
    downloader.stdout.on('data', async (data) => {
        if (data.toString("utf8").includes(lastFile)) {
            if (!finished) {
                finished = true;
                console.log("VALORANT has been successfully updated!");
                parseVersion();
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
    const parser = spawn("./tools/ValoParser.exe", [`${dataConfig.path.VALORANT}`]);
    parser.stdout.on('data', async (data) => {
        console.log(`${data.toString("utf8").replace("\n", "")}`);
    });
    parser.on("exit", async (data) => {
        gitCommit();
    })
}
async function parseManifest(manifest) {
    const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "--print-manifest"]);
    downloader.on("exit", async (data) => {
        fs.readFile(path.resolve(__dirname, manifest.replace(".manifest", ".json")),"utf-8", function(err, data) {
            if(!err) {
                fs.rename(`./${manifest.replace(".manifest", ".json")}`, './VALORANT.json', function(err) {
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
    if (!fs.existsSync("./files/manifest")) fs.mkdirSync("./files/manifest");
    try {
        fs.renameSync(`./RiotClient.json`, './files/manifest/RiotClient.json');
        fs.renameSync(`./VALORANT.json`, './files/manifest/VALORANT.json');
    } catch (e) {
        console.log(e)
    }
    const downloader = spawn("python", ["./tools/VersionParser.py", `${dataConfig.path.VALORANT}`, `${dataConfig.path.RiotClient}`]);
    downloader.stdout.on('data', async (data) => {
        console.log(`${data.toString("utf8").replace("\n", "")}`);
    });
    downloader.on("exit", async (data) => {
        console.log("Version data has been successfully parsed!");
        parseAssets();
    })
}

//Git Commit
async function gitCommit() {
    const gitAdd = spawn("git", ["add", "-A"], {cwd: "./files/"});
    gitAdd.on("exit", async () => {
        let version = fs.readFileSync("./files/version.json", "utf8");
        json = JSON.parse(version);
        const gitCommitt = spawn("git", ["commit", "-m", `${json["riotClientVersion"]}`], {cwd: "./files/"});
        gitCommitt.on("exit", async () => {
            const gitPush = spawn("git", ["push", "-u", "origin", "main"], {cwd: "./files/"});
            gitPush.on("exit", async () => {
                console.log("GitHub Repository has been successfully pushed!");
            })
        })
    })
}

setInterval(() => {
    updateRiotClientManifest();
    updateManifest();
}, 300000)
updateRiotClientManifest();
updateManifest();