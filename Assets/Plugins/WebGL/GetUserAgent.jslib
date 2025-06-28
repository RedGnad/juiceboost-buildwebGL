mergeInto(LibraryManager.library, {
  GetUserAgent: function () {
    var ua = navigator.userAgent;
    var buf = _malloc(lengthBytesUTF8(ua) + 1);
    stringToUTF8(ua, buf, lengthBytesUTF8(ua) + 1);
    return buf;
  },
});
