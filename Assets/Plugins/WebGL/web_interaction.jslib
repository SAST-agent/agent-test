mergeInto(LibraryManager.library, {

  Connect_ws: function (addrPtr) {
    var addr = UTF8ToString(addrPtr);
    console.log("[JS] Connect_ws:", addr);
    if (window.ConnectWS) window.ConnectWS(addr);
  },

  Send_ws: function (payloadPtr) {
    var payload = UTF8ToString(payloadPtr);
    console.log("[JS] Send_ws:", payload);
    if (window.SendWS) window.SendWS(payload);
  },

  Send_frontend: function (jsonPtr) {
    var json = UTF8ToString(jsonPtr);
    console.log("[JS] Send_frontend:", json);
    if (window.SendToFrontend) window.SendToFrontend(json);
  },

  Getoperation: function (index) {
    console.log("[JS] Getoperation:", index);
    if (window.SendOperation) window.SendOperation(index);
  }

});
