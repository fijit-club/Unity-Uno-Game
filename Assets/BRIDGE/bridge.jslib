const plugin = {
  setScore: function (score) {
    try {
      const data = { event: 'SET_SCORE', payload: { score } };
      window.ReactNativeWebView.postMessage(JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to dispatch event');
    }
  },
  setSound: function (isOn) {
    try {
      const data = { event: 'SOUND_CTRL', payload: { isOn } };
      if(window.ReactNativeWebView) window.ReactNativeWebView.postMessage(JSON.stringify(data));
	    if(window.dispatchReactUnityEvent) window.dispatchReactUnityEvent('gameEvent', JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to dispatch event ',e);
    }
  },
  vibrate: function (isLong) {
    try {
      const data = { event: 'VIBRATE', payload: { isLong } };
      window.ReactNativeWebView.postMessage(JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to dispatch event');
    }
  },
  restart: function () {
    try {
      const data = { event: 'RESTART' };
      window.ReactNativeWebView.postMessage(JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to dispatch event');
    }
  },
  buyAsset: function (assetId) {
    try {
      assetId = UTF8ToString(assetId);
      const data = { event: 'BUY_ASSET', payload: { assetId } };
      window.ReactNativeWebView.postMessage(JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to post message');
    }
  },
  updateCoins: function (coinsChange) {
    try {
      const data = { event: 'UPDATE_COINS', payload: coinsChange };
      window.ReactNativeWebView.postMessage(JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to post message');
    }
  },
  updateExp: function (expChange) {
    try {
      const data = { event: 'UPDATE_EXP', payload: { expChange } };
      window.ReactNativeWebView.postMessage(JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to post message');
    }
  },
setSavedata: function (savedata) {
    try {
      savedata = UTF8ToString(savedata);
      const data = { event: 'SET_SAVEDATA', payload: { savedata } };
      if(window.ReactNativeWebView) window.ReactNativeWebView.postMessage(JSON.stringify(data));
	  if(window.dispatchReactUnityEvent) window.dispatchReactUnityEvent('gameEvent', JSON.stringify(data));
	 
    } catch (e) {
      console.warn('Failed to post message');
    }
	},
  load: function () {
    try {
      const data = { event: 'LOAD' };
      window.ReactNativeWebView.postMessage(JSON.stringify(data));
    } catch (e) {
      console.warn('Failed to post message');
    }
  },
};

mergeInto(LibraryManager.library, plugin);