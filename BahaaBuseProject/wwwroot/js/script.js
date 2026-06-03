//  STATE 
let data        = []; // store jason 
let currentEra  = 0; // track what user did to store in seassion
let quizScore   = 0; // all those to track quiz info 
let quizAnswered= 0;
let quizTotal   = 0;
let activeQuiz  = [];   // holds the current era's quiz questions — read directly in answerQuiz()

//  BOOT 
async function loadData() {
    try {  // calls for bahaa data from api to show (like the information to be displayed)
        const res = await fetch('/api/HistoryApi');
        if (!res.ok) throw new Error('API ' + res.status);
        data = await res.json(); // assigning master jason
        buildTimeline(); // this renders the UI nodes the timeline that connects era
        buildEraSections(); // renders info

        // Restore last-visited era from visitor session (if user click era 3 and refersh it keeps him on it)
        let startEra = 0;
        try {
            const sessRes = await fetch('/api/SessionApi'); // callss the seassion api to see user state
            if (sessRes.ok) {
                const sess = await sessRes.json();
                if (sess.lastEra >= 0 && sess.lastEra < data.length) startEra = sess.lastEra;
            }
        } catch (_) {}

        updatePage(startEra);
    } catch (err) {
        console.error('Failed to load history data:', err); // in case of an erorr
        document.getElementById('title').innerText = 'Could not load data. Please refresh.';
    }
}

// TIMELINE 
function buildTimeline() {
    // injects nodes into  DOM based on API
    const container = document.getElementById('timeline-nodes');
    container.querySelectorAll('.node').forEach(n => n.remove());
    data.forEach(function(era, i) {
        const node = document.createElement('div');
        node.className = 'node bg-white rounded-circle position-relative' + (i === 0 ? ' active' : '');
        node.onclick = (function(idx) { return function() { updatePage(idx); }; })(i);
        node.innerHTML = '<div class="node-label position-absolute start-50 text-nowrap fw-bold">'
            + (era.nodeLabel || era.title) + '</div>';
        container.appendChild(node);
    });
}

// ERA SECTIONS (below fold)
function buildEraSections() {
    const container = document.getElementById('era-sections');
    container.innerHTML = data.map(function(era, i) {
        return '<section class="era-section py-5" id="era-section-' + i + '" style="background:' + era.bgColor + ';">'
            + '<div class="container">'
            + '<h2 class="era-heading">' + (era.sectionIcon || '') + ' ' + era.title + '</h2>'
            + (era.sectionBody || '')
            + '</div></section>';
    }).join('');

    const nav = document.getElementById('nav-era-links');
    if (nav) {
        nav.innerHTML = data.map(function(era, i) {
            return '<li class="nav-item"><a class="nav-link" href="#era-section-' + i + '">'
                + (era.nodeLabel || era.title) + '</a></li>';
        }).join('');
    }
}


//  MAIN UPDATE
function updatePage(index) {
    currentEra = index;
    const era  = data[index];

    // push UI state back to the server (changes happen in the front are stored in the back)
    fetch('/api/SessionApi/era/' + index, { method: 'POST' }).catch(() => {});
// update based on the era 
    document.getElementById('title').innerText = era.title;
    document.getElementById('desc').innerText  = era.description;
    document.body.style.backgroundColor       = era.bgColor;
    document.documentElement.style.setProperty('--accent', era.color);

    // Figures
    // creat clickable buttons for each person 
    const figContainer = document.getElementById('figures');
    figContainer.innerHTML = (era.figures || []).map(function(f) {
        return '<span onclick="showBio(' + f.id + ')">' + f.name + '</span>'; // it maps to span in html then data is collected through each person unique id
    }).join(''); //  join turns the array of HTML strings into one long string for the DOM

    // Cities same logic
    const mapContainer = document.getElementById('map-cities');
    mapContainer.innerHTML = (era.cities || []).map(function(c, i) {
        return '<button class="city-btn" onclick="showCity(' + index + ',' + i + ')">' + c.name + '</button>';
    }).join('');
    document.getElementById('city-title').innerText   = 'Select a city';
    document.getElementById('city-title').style.color = '';
    document.getElementById('city-info').innerText    = 'Click on a city to see its historical significance during this era.';


    // Stats
    document.getElementById('stat-1').style.width = era.stat1 || '75%';
    document.getElementById('stat-2').style.width = era.stat2 || '75%';
    document.getElementById('stat-3').style.width = era.stat3 || '75%';

    // Active timeline node
    document.querySelectorAll('.node').forEach(function(n, i) {
        n.classList.toggle('active', i === index);
    });

    renderQuotes(era.quotes        || []);
    renderVideos(era.videos        || []);
    renderSources(era.sources      || []);
    renderQuiz(era.quizQuestions   || []);
}

//  BIO MODAL 
function showBio(figureId) {
    //finds the specific figure by id in the data array
    // injects the figure data into the html modal containers
    // triggers the Bootstrap Modal show() command
    const era = data[currentEra];
    var f = null;
    for (var i = 0; i < era.figures.length; i++) {
        if (era.figures[i].id === figureId) { f = era.figures[i]; break; }
    }
    if (!f) return;

    document.getElementById('bioModalLabel').innerText = f.name;
    document.getElementById('bioModalBody').innerText  = f.bio;


    document.querySelector('#bioModal .modal-header').style.backgroundColor =
        getComputedStyle(document.documentElement).getPropertyValue('--accent');
    new bootstrap.Modal(document.getElementById('bioModal')).show(); // this is the bootstrap show command 
}

//  CITY INFO PANEL 
function showCity(eraIndex, cityIndex) {
    var city   = data[eraIndex].cities[cityIndex];
    var accent = getComputedStyle(document.documentElement).getPropertyValue('--accent');

    document.getElementById('city-title').innerText   = city.name;
    document.getElementById('city-title').style.color = accent;
    document.getElementById('city-info').innerText    = city.info;


    document.querySelectorAll('.city-btn').forEach(function(btn, i) {
        btn.style.backgroundColor = (i === cityIndex) ? accent : '#fff';
        btn.style.color           = (i === cityIndex) ? '#fff' : '#333';
        btn.style.borderColor     = accent;
    });
}

//  QUOTES
function renderQuotes(quotes) {
    var c = document.getElementById('quotes-container'); // find html conatainer 
    if (!c) return; // if dosent exist stop to aviod erorr
    c.innerHTML = quotes.map(function(q) { // loop
        return '<div class="quote-card">' // creat html string for each quote
            + '<div class="quote-text">\u201c' + q.text + '\u201d</div>'
            + '<div class="quote-author">\u2014 ' + q.author + '</div>'
            + '</div>';
    }).join('');
}

//  VIDEOS 
function renderVideos(videos) {
    var c = document.getElementById('videos-container');
    if (!c) return;
    c.innerHTML = videos.map(function(v) {
        // creat clickable link 
        return '<a href="' + v.url + '" target="_blank" rel="noopener noreferrer" class="video-card">'
            + '<div class="video-thumb">&#9654;</div>' // play icon
            + '<div><div class="video-title">' + v.title + '</div>'
            + '<div class="video-channel">YouTube Research</div></div>'
            + '</a>';
    }).join('');
}

//  SOURCES
function renderSources(sources) {
    var c = document.getElementById('sources-container');
    if (!c) return;
    c.innerHTML = sources.map(function(s) {
        // creat a badge for eahc link of sources 
        return '<a href="' + s.url + '" target="_blank" rel="noopener noreferrer" class="source-badge">'
            + '&#128218; ' + s.label + '</a>';
    }).join('');
}

// QUIZ 
function renderQuiz(questions) {
    quizScore    = 0; // to reset score for each era 
    quizAnswered = 0;

    // shuffle a copy and take up to 10
    activeQuiz = questions.slice().sort(function() { return Math.random() - 0.5; }).slice(0, 10);
    quizTotal  = activeQuiz.length;

    var wrapper = document.getElementById('quiz-wrapper');
    if (!wrapper) return;

    if (activeQuiz.length === 0) {
        wrapper.innerHTML = '<p class="text-muted">No quiz questions available for this era yet.</p>';
        return;
    }

    // quiz ui
    var html = activeQuiz.map(function(q, qi) { // loop through questions
        var optHtml = (q.options || []).map(function(opt, oi) { 
            // button is answerQuiz() with two IDs: which question and which answer
            return '<button class="quiz-option" onclick="answerQuiz(' + qi + ',' + oi + ')">'
                + opt.text + '</button>';
        }).join('');

        return '<div class="quiz-question" id="quiz-q-' + qi + '">'
            + '<p><strong>' + (qi + 1) + '.</strong> ' + q.question + '</p>'
            + optHtml
            + '</div>';
    }).join('');

    html += '<div id="quiz-score" style="display:none;"></div>';
    wrapper.innerHTML = html;
}

function answerQuiz(qIndex, chosen) {
    var q         = activeQuiz[qIndex];
    var correct   = q.correctIndex;               // read from JS array — never from HTML
    var el        = document.getElementById('quiz-q-' + qIndex);
    if (!el) return;

    var buttons = el.querySelectorAll('.quiz-option');
    if (buttons.length === 0 || buttons[0].disabled) return;   // already answered

    buttons.forEach(function(btn, i) {
        btn.disabled = true;
        if (i === correct)     btn.classList.add('correct'); // compare user choice to the correct index
        else if (i === chosen) btn.classList.add('wrong');
    });

    if (chosen === correct) quizScore++;
    quizAnswered++;

    if (quizAnswered === quizTotal) {
        var scoreEl = document.getElementById('quiz-score');
        if (!scoreEl) return;
        scoreEl.style.display = 'block';
        var pct   = Math.round((quizScore / quizTotal) * 100);
        var emoji = pct === 100 ? '🏆' : pct >= 50 ? '👍' : '📚';
        scoreEl.innerHTML = emoji + ' You scored <strong>' + quizScore + ' / ' + quizTotal
            + '</strong> (' + pct + '%) \u2014 '
            + (pct === 100 ? 'Perfect score!' : pct >= 50 ? 'Well done!' : 'Keep exploring!');
    }
}

// ENTRY POINT
window.onload = loadData;