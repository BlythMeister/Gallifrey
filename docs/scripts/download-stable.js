$(document).ready(function () {
    $.getJSON("https://api.github.com/repos/BlythMeister/Gallifrey/releases/latest").done(function (json) {		
		var downloadURL = "https://github.com/BlythMeister/Gallifrey/releases/latest";		
		$.each(json.assets, function() {
			if(this.name === "stable-setup.exe")
			{
				downloadURL = this.browser_download_url;
			}
		})                  
        window.location.href = downloadURL;
        $("a[id='downloadLink']").attr('href', downloadURL)
    }); 
});