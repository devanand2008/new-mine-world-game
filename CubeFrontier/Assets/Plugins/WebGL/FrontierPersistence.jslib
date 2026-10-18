mergeInto(LibraryManager.library, {
  FrontierSync: function (namePtr) {
    var objectName = UTF8ToString(namePtr);
    FS.syncfs(false, function (error) {
      SendMessage(objectName, 'SaveSynced', error ? String(error) : '');
    });
  }
});
