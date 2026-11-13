window.lines98Storage = {
    read: () => { try { return localStorage.getItem('lines98.bestScore'); } catch { return null; } },
    write: value => { try { localStorage.setItem('lines98.bestScore', value); } catch { } }
};
