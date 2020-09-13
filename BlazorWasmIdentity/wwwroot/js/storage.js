
window.getFromStorage = (key) => {
    return localStorage.getItem(key);
};

window.saveToStorage = (key, data) => {
    localStorage.setItem(key, data);
    return true;
};

window.removeItemFromStorage = (key) => {
    return localStorage.removeItem(key);
};