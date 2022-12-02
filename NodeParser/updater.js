const spawn = require("child_process").spawn;
const fs = require("fs");
const path = require("path");
const https = require('https');

class updater {
    async updateGame(lastFile, manifest) {
        let finished = false;
        const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "-b", "https://valorant.secure.dyn.riotcdn.net/channels/public/bundles", "-t", "8", "-o", "Z:/Valorant"]);
        downloader.stdout.on('data', async (data) => {
            if (data.toString("utf8").includes(lastFile)) {
                if (!finished) {
                    finished = true;
                    console.log("Game files has been successfully updated!");
                    this.parseAssets();
                }
            } else {
                console.log(data.toString("utf8"));
            }
        });
        downloader.on("exit", async (data) => {
            if (!finished) {
                this.updateGame(lastFile, manifest);
            }
        })
    }
    
    async parseAssets() {
        const parser = spawn("./tools/ValoParser.exe", ["Z:/Valorant"]);
        parser.stdout.on('data', async (data) => {
            console.log(`${data.toString("utf8").replaceLast("\n", "")}`);
        });
        parser.on("exit", async (data) => {
            fs.rename(`./manifest.json`, './files/manifest.json', function(err) {
                if (!err) {
                    console.log("API data has been successfully updated!");
                }
            });
        })
    }
    
    async parseManifest(manifest) {
        const downloader = spawn("./tools/ManifestDownloader.exe", [manifest, "--print-manifest"]);
        downloader.on("exit", async (data) => {
            fs.readFile(path.resolve(__dirname, manifest.replace(".manifest", ".json")),"utf-8", function(err, data) {
                if(!err) {
                    fs.rename(`./${manifest.replace(".manifest", ".json")}`, './manifest.json', function(err) {
                        if (!err) {
                            let json = JSON.parse(data);
                            console.log("Mainfest has been successfully parsed!");
                            this.updateGame(json.files[json.files.length - 1].path, manifest);
                        }
                    });
                }
            })
        })
    }
    
    async updateManifest() {
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
                        if (!fs.existsSync(`./${item["patch_url"].split("/")[item["patch_url"].split("/").length-1]}`)) {
                            const file = fs.createWriteStream(item["patch_url"].split("/")[item["patch_url"].split("/").length-1]);
                            https.get(item["patch_url"], function(res) {
                                res.pipe(file);
                                file.on("finish", () => {
                                    file.close();
                                    console.log("Mainfest has been successfully updated!");
                                    this.parseManifest(item["patch_url"].split("/")[item["patch_url"].split("/").length-1]);
                                });
                            });
                        }
                    }
                }
            });
        })
    }
}

module.exports = new updater();