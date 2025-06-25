window.Muse = {
  ReadCookie(name) {
    return Cookies.get(name);
  },

  WriteCookie(name, value, expires) {
    Cookies.set(name, value, { expires })
  },
};