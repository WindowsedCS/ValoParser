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
        pattern = "++Ares-Core+release-".encode("utf-16le")
        patternBuild = "ProductVersion".encode("utf-16le")

        # Formatting Data
        pos = data.find(pattern) + len(pattern)
        fullData = data[pos:pos+100].decode("utf-16le").rstrip("\x00").split("\u0000")
        parsedData = []
        for i in range(0, len(fullData)):
            if fullData[i] != "":
                parsedData.append(fullData[i])

        # VALORANT Branch
        branch = "release-{0}".format(parsedData[0].rstrip("\x00"))
        resJson["branch"] = branch

        # Build Date
        date = parsedData[1].rstrip("\x00").split(" ")
        parsedDate = []
        for i in range(0, len(date)):
            if date[i] != "":
                parsedDate.append(date[i])
        month =  parsedDate[0]
        day =  parsedDate[1]
        year =  parsedDate[2]
        buildDate = "{0}-{1}-{2}T00:00:00.000Z".format(add_zero(year), add_zero(json1[month]), add_zero(day))

        # VALORANT Version
        if "." in parsedData[2].rstrip("\x00"):
            version = parsedData[2].rstrip("\x00")
        else:
            version = parsedData[3].rstrip("\x00")
        resJson["version"] = version

        # Build Version
        pos = data.find(patternBuild) + len(patternBuild) + 2
        buildVersion = data[pos:pos+4].decode("utf-16le").rstrip("\x00")
        resJson["buildVersion"] = buildVersion

        # Riot Client Version
        riotClientVersion = "{0}-shipping-{1}-{2}".format(branch, buildVersion, version.split(".").pop())
        resJson["riotClientVersion"] = riotClientVersion

        # Riot Client Build
        get_riot_client_version()
        resJson["buildDate"] = buildDate.replace(" ", "0")
        save_json()

def get_riot_client_version(path = "{0}/RiotClientServices.exe".format(sys.argv[2]), path1 = "{0}/RiotGamesApi.dll".format(sys.argv[2])):
    with open(path, "rb") as exe_file:
        # Riot Client Build
        data = exe_file.read()
        pattern = "FileVersion".encode("utf-16le")

        pos = data.find(pattern) + len(pattern)
        fullData = data[pos:pos+100].decode("utf-16le").rstrip("\x00").split("\u0000")
        parsedData = []
        for i in range(0, len(fullData)):
            if fullData[i] != "":
                parsedData.append(fullData[i])

        riotClientVersion = parsedData[0]
        
        # Riot Api Version
        with open(path1, "rb") as exe_file:
            data = exe_file.read()
            pattern = "FileVersion".encode("utf-16le")

            pos = data.find(pattern) + len(pattern)
            fullData = data[pos:pos+100].decode("utf-16le").rstrip("\x00").split("\u0000")
            parsedData = []
            for i in range(0, len(fullData)):
                if fullData[i] != "":
                    parsedData.append(fullData[i])
            apiVersion = parsedData[0]
            apiVersion = apiVersion.split(".")[3]

            riotClientBuild = riotClientVersion + "." + apiVersion
            resJson["riotClientBuild"] = riotClientBuild

def save_json():
    with open("./files/version.json", "w") as outfile:
        json.dump(resJson, outfile)

def add_zero(number):
    if len(str(number)) == 1:
        return "0" + str(number)
    else:
        return str(number)

get_valorant_version()