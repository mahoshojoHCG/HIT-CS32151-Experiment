
var preferVideo = {
    addItem: function(videoId) {
        if (!videoId)
            return false;
        var current = docCookies.getItem("preferVideo");
        var currentPrefer;
        if (!current) {
            currentPrefer = [videoId];
        } else {
            currentPrefer = current.split(",");
            if (currentPrefer.includes(videoId))
                return false;
            currentPrefer.push(videoId);
        }
        docCookies.setItem("preferVideo", currentPrefer.join(","));
        return true;

    },
    deleteItem: function(videoId) {
        if (!videoId)
            return false;
        var current = docCookies.getItem("preferVideo");
        var currentPrefer;
        if (!current) {
            return false;
        } else {
            currentPrefer = current.split(",");
            docCookies.setItem("preferVideo", currentPrefer.slice(videoId).join(","));
            return true;
        }
    },
    hasItem: function(videoId) {
        if (!videoId)
            return false;
        var current = docCookies.getItem("preferVideo");
        if (!current) {
            return false;
        } else {
            return current.split(",").includes(videoId);
        }
    }
}