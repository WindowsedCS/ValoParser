import sys
import json

json1 = {
    "Jan": 1,
    "Feb": 2,
    "Mar": 3,
    "Apr": 4,
    "May": 5,
    "Jun": 6,
    "Jul": 7,
    "Aug": 8,
    "Sep": 9,
    "Oct": 10,
    "Nov": 11,
    "Dec": 12
}

resJson = {}

def get_valorant_version(path = "{0}/ShooterGame/Binaries/Win64/VALORANT-Win64-Shipping.exe".format(sys.argv[1])):
    with open(path, "rb") as exe_file:
        # Manifest ID
        file = open('./files/manifest/VALORANT.json')
        manifest = json.load(file)
        resJson["manifestId"] = manifest["Manifest ID"]
        # Parse EXE file
        data = exe_file.read()
        # VALORANT Branch
        pattern = "++Ares-Core+release-".encode("utf-16le")
        pos = data.find(pattern) + len(pattern)
        branch = "release-{0}".format(data[pos:pos+16].decode("utf-16le").rstrip("\x00"))
        resJson["branch"] = branch
        # VALORANT Version
        pos = data.find(pattern) + len(pattern) + 48
        version = data[pos:pos+32].decode("utf-16le").rstrip("\x00")
        resJson["version"] = version
        # Build Version
        pos = data.find(pattern) + len(pattern) + 40
        buildVersion = data[pos:pos+6].decode("utf-16le").rstrip("\x00")
        resJson["buildVersion"] = buildVersion
        # Riot Client Version
        riotClientVersion = "{0}-shipping-{1}-{2}".format(branch, buildVersion, version.split(".").pop())
        resJson["riotClientVersion"] = riotClientVersion
        # Riot Client Build
        get_riot_client_version()
        # Build Date
        pos = data.find(pattern) + len(pattern) + 16
        month =  data[pos:pos+6].decode("utf-16le").rstrip("\x00")
        day =  data[pos+8:pos+12].decode("utf-16le").rstrip("\x00")
        year =  data[pos+14:pos+22].decode("utf-16le").rstrip("\x00")
        buildDate = "{0}-{1}-{2}T00:00:00.000Z".format(year, json1[month], day)
        resJson["buildDate"] = buildDate

def get_riot_client_version(path = "{0}/RiotClientServices.exe".format(sys.argv[2]), path1 = "{0}/RiotGamesApi.dll".format(sys.argv[2])):
    with open(path, "rb") as exe_file:
        # Riot Client Build
        data = exe_file.read()
        pattern = "FileVersion".encode("utf-16le")
        pos = data.find(pattern) + len(pattern) + 4
        riotClientVersion = data[pos:pos+30].decode("utf-16le").rstrip("\x00")
        # Riot Api Version
        with open(path1, "rb") as exe_file:
            data = exe_file.read()
            pattern = "FileVersion".encode("utf-16le")
            pos = data.find(pattern) + len(pattern) + 20
            riotClientBuild = riotClientVersion + "." + data[pos:pos+14].decode("utf-16le").rstrip("\x00")
            resJson["riotClientBuild"] = riotClientBuild
            save_json()

def save_json():
    with open("./files/version.json", "w") as outfile:
        json.dump(resJson, outfile)

get_valorant_version()