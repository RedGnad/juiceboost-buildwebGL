mergeInto(LibraryManager.library, {
  GetUserAgent: function () {
    var userAgent = navigator.userAgent;
    var bufferSize = lengthBytesUTF8(userAgent) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(userAgent, buffer, bufferSize);
    return buffer;
  },
});
