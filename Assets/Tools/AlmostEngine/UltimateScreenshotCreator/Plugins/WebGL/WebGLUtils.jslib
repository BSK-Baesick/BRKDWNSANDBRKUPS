
var ImageDownloaderPlugin = {
	
	_ExportImage: function(data, filename, format) {

		var imageData = Pointer_stringify(data);
		var imageFileName = Pointer_stringify(filename);
		var contentType = 'image/'+ format;
		
		// Converts the image data to binary
		// From http://stackoverflow.com/questions/14967647/
		// encode-decode-image-with-base64-breaks-image (2013-04-21)
		function fixBinary (data) {
			var length = data.length;
			var binary = new ArrayBuffer(length);
			var arr = new Uint8Array(binary);
			for (var i = 0; i < length; i++)
			{
				arr[i] = data.charCodeAt(i);
			}
			return binary;
		}
		var binary = fixBinary(atob(imageData));
		
		// Creates an image Blob
		var imageBlob = new Blob([binary], {type: contentType});
		
		// Creates a clickable link that will download the image
		var link = document.createElement('a');
		link.download = imageFileName;
		link.innerHTML = 'DownloadFile';
		link.setAttribute('id', 'ImageDownloaderLink');
		
		// Creates the click URL
		if(window.webkitURL != null) {
			link.href = window.webkitURL.createObjectURL(imageBlob);
		}
		else {
			link.href = window.URL.createObjectURL(imageBlob);
			// Creates the click function
			link.onclick = function()
			{
				var child = document.getElementById('ImageDownloaderLink');
				child.parentNode.removeChild(child);
			};
			link.style.display = 'none';
			document.body.appendChild(link);
		}		
		
		//Calling the link click action
		link.click();	
	}	

};

mergeInto(LibraryManager.library, ImageDownloaderPlugin);