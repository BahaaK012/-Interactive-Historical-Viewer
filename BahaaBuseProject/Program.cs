using Microsoft.EntityFrameworkCore;
using BahaaBuseProject.Data;
using BahaaBuseProject.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

/* setup json to handle object loops so it doesnt crash */
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // makes json match camelCase names script.js uses
    });

/* db — stores the file locally so it just works */
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "history.db");
builder.Services.AddDbContext<HistoryContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

/* session — tracks visitor stuff like era progress */
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout         = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly     = true;
    options.Cookie.IsEssential  = true;
    options.Cookie.SameSite     = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

/* cookie auth — lets admin login and stay logged in */
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath         = "/Admin/Login";
        options.LogoutPath        = "/Admin/Logout";
        options.AccessDeniedPath  = "/Admin/Login";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly   = true;
        options.Cookie.SameSite   = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

var app = builder.Build();

/* db init — makes sure the db exists and has data */
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HistoryContext>();
    db.Database.EnsureCreated();
    if (!db.Eras.Any())
        SeedDatabase(db); // fills it up if empty
    else
        EnsureQuizQuestions(db); // adds missing quiz questions
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting();
app.UseSession();           
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


/* seeder */
static void SeedDatabase(HistoryContext ctx)
{
    /* era 1 — abbasid caliphate */
    var abbasid = new Era
    {
        Title = "The Abbasid Caliphate", NodeLabel = "The Abbasids",
        Description = "The golden age of the Islamic Empire centered in Baghdad — a civilization that preserved ancient knowledge, pushed the boundaries of science and philosophy, and established the House of Wisdom as the intellectual capital of the medieval world.",
        Color = "#d4a373", BgColor = "#fefae0",
        Stat1 = "95%", Stat2 = "85%", Stat3 = "70%",
        SectionIcon = "✨",
        SectionBody = "<p>Spanning 750–1258 CE, the Abbasid Caliphate transformed Baghdad into the intellectual capital of the world.</p><ul><li>Founded in 750 CE after overthrowing the Umayyad Caliphate.</li><li>Baghdad's population exceeded one million at its peak.</li><li>Al-Khwarizmi's work gave us the word \"algebra\".</li><li>The Translation Movement brought Greek, Persian, and Indian texts into Arabic.</li></ul>",
        Figures = new List<Figure>
        {
            new() { Name = "Harun al-Rashid", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a2/Harun_al-Rashid_-_Imaginary_portrait.jpg/440px-Harun_al-Rashid_-_Imaginary_portrait.jpg",
                Bio = "The fifth Abbasid caliph (r. 786–809 CE), Harun al-Rashid presided over the golden age of the caliphate. He established the House of Wisdom in Baghdad, patronized poets and scholars, and maintained diplomatic relations with Charlemagne. His court became legendary, later immortalized in the tales of One Thousand and One Nights." },
            new() { Name = "Al-Khwarizmi", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/19/Al-Khwarizmi_portrait.jpg/440px-Al-Khwarizmi_portrait.jpg",
                Bio = "Muhammad ibn Musa al-Khwarizmi (c. 780–850 CE) was a mathematician, astronomer, and scholar at the House of Wisdom. His treatise introduced algebra to the world, and his name gave rise to the word 'algorithm'. He also advanced trigonometry and produced revised geographical tables." },
            new() { Name = "Al-Kindi", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c1/Al-kindi.jpg/440px-Al-kindi.jpg",
                Bio = "Abu Yusuf Yaqub ibn Ishaq al-Kindi (c. 801–873 CE) is widely regarded as the first philosopher in the Arabic tradition. A prolific polymath, he wrote over 260 works on philosophy, mathematics, optics, cryptography, music theory, and meteorology." },
            new() { Name = "Ibn Sina", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/84/Avicennapersia.jpg/440px-Avicennapersia.jpg",
                Bio = "Abu Ali ibn Sina (980–1037 CE), known in the West as Avicenna, was one of the most significant physicians and philosophers of the medieval world. His Canon of Medicine remained the standard medical textbook in European universities until the 17th century." }
        },
        Cities = new List<City>
        {
            new() { Name = "Baghdad", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/74/Tigr_baghdad.jpg/640px-Tigr_baghdad.jpg",
                Info = "Founded in 762 CE by Caliph Al-Mansur as a perfectly circular city, Baghdad was the largest and most sophisticated metropolis in the medieval world. At its peak, it housed over one million inhabitants, the legendary House of Wisdom, vast markets, and a complex canal system." },
            new() { Name = "Damascus", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/97/Umayyad_Mosque%2C_Damascus.jpg/640px-Umayyad_Mosque%2C_Damascus.jpg",
                Info = "The former capital of the Umayyad Caliphate, Damascus retained its immense importance under Abbasid rule as a commercial and agricultural powerhouse of the Levant. Its famous bazaars traded in textiles, spices, metalwork, and glass." },
            new() { Name = "Samarra", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Malwiya_mosque.jpg/640px-Malwiya_mosque.jpg",
                Info = "Built as a new imperial capital between 836 and 892 CE, Samarra stretched over 57 km along the Tigris. It is famed for the iconic spiral minaret of the Great Mosque of Al-Mutawakkil — one of the most recognizable structures of the Islamic world." }
        },
        Quotes = new List<Quote>
        {
            new() { Text = "Seek knowledge, even if you must travel to China.", Author = "Attributed to the Prophet Muhammad, widely invoked in Abbasid scholarship" },
            new() { Text = "The ink of a scholar is more sacred than the blood of a martyr.", Author = "Islamic scholarly tradition, reflecting Abbasid intellectual values" },
            new() { Text = "He who does not know mathematics cannot fully know any other science.", Author = "Al-Kindi, philosopher of the House of Wisdom" }
        },
        Videos = new List<Video>
        {
            new() { Title = "The Abbasid Caliphate: Islam's Golden Age", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=abbasid+caliphate+golden+age" },
            new() { Title = "House of Wisdom: How Islam Saved Knowledge", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=house+of+wisdom+baghdad+documentary" },
            new() { Title = "Al-Khwarizmi: Father of Algebra", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=al+khwarizmi+algebra+ted+ed" }
        },
        Sources = new List<Source>
        {
            new() { Label = "Wikipedia: Abbasid Caliphate", Url = "https://en.wikipedia.org/wiki/Abbasid_Caliphate" },
            new() { Label = "Britannica: House of Wisdom", Url = "https://www.britannica.com/topic/House-of-Wisdom" },
            new() { Label = "Al-Khwarizmi — MacTutor History", Url = "https://mathshistory.st-andrews.ac.uk/Biographies/Al-Khwarizmi/" },
            new() { Label = "Avicenna: Stanford Encyclopedia", Url = "https://plato.stanford.edu/entries/ibn-sina/" }
        },
        QuizQuestions = new List<QuizQuestion>
        {
            new() { Question = "What was the name of the great academic institution founded in Baghdad during the Abbasid Caliphate?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Al-Azhar University" }, new() { Text = "The House of Wisdom" }, new() { Text = "The Grand Academy" }, new() { Text = "The School of Baghdad" } } },
            new() { Question = "Which Abbasid scholar is known as the 'father of algebra' for his mathematical treatise?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "Al-Kindi" }, new() { Text = "Ibn Sina" }, new() { Text = "Al-Khwarizmi" }, new() { Text = "Al-Biruni" } } },
            new() { Question = "In what year was the Abbasid Caliphate founded?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "622 CE" }, new() { Text = "750 CE" }, new() { Text = "830 CE" }, new() { Text = "900 CE" } } },
            new() { Question = "Who founded Baghdad in 762 CE?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "Harun al-Rashid" }, new() { Text = "Al-Kindi" }, new() { Text = "Caliph Al-Mansur" }, new() { Text = "Al-Khwarizmi" } } },
            new() { Question = "What does the word 'algorithm' derive from?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "The Latinized name of Al-Khwarizmi" }, new() { Text = "An Arabic word for calculation" }, new() { Text = "A Greek mathematical term" }, new() { Text = "The name of a Baghdad school" } } },
            new() { Question = "Which Western name is Ibn Sina known by?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "Averroes" }, new() { Text = "Alhazen" }, new() { Text = "Albumasar" }, new() { Text = "Avicenna" } } },
            new() { Question = "What was the Translation Movement in Abbasid Baghdad?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "A program to translate the Quran into all languages" }, new() { Text = "A systematic effort to translate Greek, Persian, and Indian texts into Arabic" }, new() { Text = "A school that taught foreign languages to diplomats" }, new() { Text = "A movement to spread Arabic literature to Europe" } } },
            new() { Question = "Which empire did the Abbasids overthrow to seize the caliphate?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "The Umayyad Caliphate" }, new() { Text = "The Sasanian Empire" }, new() { Text = "The Byzantine Empire" }, new() { Text = "The Fatimid Caliphate" } } },
            new() { Question = "In what year did the Mongol invasion destroy Baghdad and end the Abbasid Caliphate?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "1187 CE" }, new() { Text = "1204 CE" }, new() { Text = "1258 CE" }, new() { Text = "1301 CE" } } },
            new() { Question = "Al-Kindi is credited as the first philosopher in which tradition?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Persian philosophical tradition" }, new() { Text = "Arabic philosophical tradition" }, new() { Text = "Ottoman philosophical tradition" }, new() { Text = "Byzantine philosophical tradition" } } }
        }
    };

    /* era 2 — ottoman empire */
    var ottoman = new Era
    {
        Title = "Ottoman Empire", NodeLabel = "Ottoman Empire",
        Description = "A transcontinental empire of extraordinary longevity, the Ottomans bridged East and West for six centuries — forging an administrative and legal system of remarkable sophistication, producing architectural masterpieces of enduring beauty, and shaping the political geography of three continents.",
        Color = "#606c38", BgColor = "#f2f4ef",
        Stat1 = "75%", Stat2 = "90%", Stat3 = "95%",
        SectionIcon = "⚔️",
        SectionBody = "<p>From 1299 to 1922, the Ottoman Empire bridged East and West for over six centuries.</p><ul><li>Mehmed II conquered Constantinople in 1453, ending the Byzantine Empire.</li><li>Suleiman I oversaw the Süleymaniye Mosque by Mimar Sinan.</li><li>The empire administered 30+ modern nations at its height.</li><li>Tanzimat reforms introduced constitutional governance.</li></ul>",
        Figures = new List<Figure>
        {
            new() { Name = "Mehmed II", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8e/Gentile_Bellini_003.jpg/440px-Gentile_Bellini_003.jpg",
                Bio = "Mehmed II (1432–1481), known as the Conqueror, became Sultan at nineteen and captured Constantinople in 1453 after a 53-day siege. A polyglot who spoke six languages, Mehmed was as much a scholar and patron of the arts as a military commander." },
            new() { Name = "Suleiman I", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/ef/Suleiman_the_Magnificent2.jpg/440px-Suleiman_the_Magnificent2.jpg",
                Bio = "Suleiman I (1494–1566) ruled the Ottoman Empire at the height of its power for 46 years. Known in the West as 'the Magnificent' and among his own subjects as 'the Lawgiver', he codified Ottoman law and oversaw the empire's greatest territorial expansion." },
            new() { Name = "Mimar Sinan", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Sinan.jpg/440px-Sinan.jpg",
                Bio = "Mimar Sinan (c. 1489–1588) served as chief architect to three successive sultans and produced over 300 structures. His Süleymaniye Mosque in Istanbul and Selimiye Mosque in Edirne are considered among the greatest architectural achievements of the 16th century." },
            new() { Name = "Hayreddin Barbarossa", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/57/Hayreddin_Barbarossa.jpg/440px-Hayreddin_Barbarossa.jpg",
                Bio = "Hayreddin Barbarossa (c. 1478–1546) was the most feared and celebrated admiral of the 16th century Mediterranean. He rose from piracy to command the entire Ottoman navy, securing dominance over the Mediterranean and North Africa." }
        },
        Cities = new List<City>
        {
            new() { Name = "Istanbul", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/10/Bosporus_Bridge_3.jpg/640px-Bosporus_Bridge_3.jpg",
                Info = "Conquered as Constantinople in 1453 and renamed Istanbul, the city was transformed by Mehmed II into a fitting imperial capital, adorned with the Topkapı Palace, the Blue Mosque, and the Grand Bazaar — the largest covered market in the world." },
            new() { Name = "Bursa", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e5/Green_Mosque_Bursa.jpg/640px-Green_Mosque_Bursa.jpg",
                Info = "The first major Ottoman capital, Bursa was captured in 1326 and distinguished itself as a centre of commerce and culture through the silk trade. It remains home to some of the finest early Ottoman architecture including the Green Mosque and Green Tomb." },
            new() { Name = "Cairo", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/af/All_Gizah_Pyramids.jpg/640px-All_Gizah_Pyramids.jpg",
                Info = "After the Ottoman conquest of Egypt in 1517, Cairo became one of the most important cities in the empire — a provincial capital governing all of North Africa and a vital node in trade networks linking the Mediterranean to the Red Sea." }
        },
        Quotes = new List<Quote>
        {
            new() { Text = "I am the sultan of sultans, the sovereign of sovereigns, the distributor of crowns to the monarchs of the globe.", Author = "Suleiman the Magnificent, in a letter to King Francis I of France" },
            new() { Text = "To make a great empire, it is necessary to move the capital.", Author = "Mehmed II, on his transformation of Constantinople" },
            new() { Text = "A building is not truly beautiful unless it serves a purpose.", Author = "Mimar Sinan, attributed" }
        },
        Videos = new List<Video>
        {
            new() { Title = "Rise of the Ottoman Empire", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=ottoman+empire+rise+documentary" },
            new() { Title = "Suleiman the Magnificent: The Greatest Sultan", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=suleiman+magnificent+documentary" },
            new() { Title = "The Fall of Constantinople 1453", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=fall+of+constantinople+1453+documentary" }
        },
        Sources = new List<Source>
        {
            new() { Label = "Wikipedia: Ottoman Empire", Url = "https://en.wikipedia.org/wiki/Ottoman_Empire" },
            new() { Label = "Britannica: Suleiman the Magnificent", Url = "https://www.britannica.com/biography/Suleiman-the-Magnificent" },
            new() { Label = "Stanford Encyclopedia: Ottoman Architecture", Url = "https://plato.stanford.edu/entries/ottoman/" },
            new() { Label = "Cambridge History of Islam", Url = "https://www.cambridge.org/core/books/cambridge-history-of-islam/9CE1BB98DA745B3BE822DB31C2C33F9B" }
        },
        QuizQuestions = new List<QuizQuestion>
        {
            new() { Question = "In what year did Ottoman Sultan Mehmed II conquer Constantinople?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "1389" }, new() { Text = "1453" }, new() { Text = "1517" }, new() { Text = "1529" } } },
            new() { Question = "Which Ottoman architect designed both the Süleymaniye Mosque and the Selimiye Mosque?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Sinan Bey" }, new() { Text = "Mimar Sinan" }, new() { Text = "Hayreddin" }, new() { Text = "Mehmed Aga" } } },
            new() { Question = "How long did Suleiman I reign as Ottoman Sultan?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "20 years" }, new() { Text = "35 years" }, new() { Text = "46 years" }, new() { Text = "60 years" } } },
            new() { Question = "By what nickname was Suleiman I known in Western Europe?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "The Magnificent" }, new() { Text = "The Conqueror" }, new() { Text = "The Lawgiver" }, new() { Text = "The Builder" } } },
            new() { Question = "Which city served as the first major capital of the Ottoman Empire before Istanbul?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "Ankara" }, new() { Text = "Edirne" }, new() { Text = "Konya" }, new() { Text = "Bursa" } } },
            new() { Question = "What was the Kanun, as associated with Suleiman I?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "The name of Suleiman's palace" }, new() { Text = "A codification of Ottoman secular law" }, new() { Text = "The Ottoman navy's battle flag" }, new() { Text = "A famous Ottoman musical instrument" } } },
            new() { Question = "Hayreddin Barbarossa secured Ottoman dominance in the Mediterranean at which battle?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "Battle of Lepanto" }, new() { Text = "Battle of Mohács" }, new() { Text = "Battle of Preveza" }, new() { Text = "Battle of Vienna" } } },
            new() { Question = "In which year did the Ottoman Empire officially end?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "1908" }, new() { Text = "1912" }, new() { Text = "1918" }, new() { Text = "1922" } } },
            new() { Question = "How many languages was Mehmed II reputed to speak?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Three" }, new() { Text = "Six" }, new() { Text = "Eight" }, new() { Text = "Ten" } } },
            new() { Question = "The Ottoman conquest of Egypt in 1517 brought which city under Ottoman rule?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "Cairo" }, new() { Text = "Alexandria" }, new() { Text = "Mecca" }, new() { Text = "Jerusalem" } } }
        }
    };

    /* era 3 — american era */
    var american = new Era
    {
        Title = "American Era", NodeLabel = "American Era",
        Description = "Born in revolutionary idealism and forged through industrialization, civil war, and two World Wars, the United States rose within two centuries to become the most powerful and culturally influential nation in recorded history.",
        Color = "#2b2d42", BgColor = "#edf2f4",
        Stat1 = "90%", Stat2 = "95%", Stat3 = "80%",
        SectionIcon = "🇺🇸",
        SectionBody = "<p>Born in revolution and forged through industrialization, the United States emerged as the defining superpower of the 20th and 21st centuries.</p><ul><li>The Declaration of Independence was signed July 4, 1776.</li><li>Lincoln's Emancipation Proclamation began the end of slavery in 1863.</li><li>Edison held over 1,093 patents.</li><li>The U.S. landed on the Moon in 1969.</li></ul>",
        Figures = new List<Figure>
        {
            new() { Name = "George Washington", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b6/Gilbert_Stuart_Williamstown_Portrait_of_George_Washington.jpg/440px-Gilbert_Stuart_Williamstown_Portrait_of_George_Washington.jpg",
                Bio = "George Washington (1732–1799) was the indispensable man of the American founding — commander of the Continental Army during the Revolutionary War and the first President of the United States. His decision to voluntarily relinquish power after two terms set a precedent for democratic governance." },
            new() { Name = "Abraham Lincoln", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/ab/Abraham_Lincoln_O-77_matte_collodion_print.jpg/440px-Abraham_Lincoln_O-77_matte_collodion_print.jpg",
                Bio = "Abraham Lincoln (1809–1865) guided the United States through its most existential crisis — the Civil War — with political brilliance, moral clarity, and profound personal compassion. His Emancipation Proclamation of 1863 transformed the war into a struggle for human freedom." },
            new() { Name = "Thomas Edison", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9d/Thomas_Edison2.jpg/440px-Thomas_Edison2.jpg",
                Bio = "Thomas Alva Edison (1847–1931) was the most prolific inventor in American history, holding 1,093 patents. His invention of a practical incandescent light bulb in 1879 and the first electric power distribution system in 1882 fundamentally transformed modern civilization." },
            new() { Name = "Neil Armstrong", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0d/Neil_Armstrong_pose.jpg/440px-Neil_Armstrong_pose.jpg",
                Bio = "Neil Armstrong (1930–2012) became the first human being to set foot on the Moon on July 20, 1969, during the Apollo 11 mission — the culmination of a decade-long national effort and the most audacious technological achievement in human history." }
        },
        Cities = new List<City>
        {
            new() { Name = "Washington D.C.", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/af/Washington_Capitol_Dome_crop.jpg/640px-Washington_Capitol_Dome_crop.jpg",
                Info = "Designed from scratch by French engineer Pierre Charles L'Enfant, Washington D.C. became the political capital of the United States in 1800. Its monumental neoclassical architecture — the Capitol, the White House, the Lincoln Memorial — expresses the democratic ideals of the new republic." },
            new() { Name = "New York City", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/05/Southwest_corner_of_Central_Park%2C_looking_east%2C_NYC.jpg/640px-Southwest_corner_of_Central_Park%2C_looking_east%2C_NYC.jpg",
                Info = "New York City grew from a Dutch trading post into the financial and cultural capital of the world. Wall Street became the nerve center of global finance, while Broadway, Harlem, and Greenwich Village shaped the cultural landscape of the 20th century." },
            new() { Name = "Chicago", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/be/Chicago_bw_Photo.jpg/640px-Chicago_bw_Photo.jpg",
                Info = "Chicago rose from a small frontier settlement to the second-largest American city in sixty years. Its transcontinental railroad hub made it the great processor of American agriculture and industry. Chicago architects invented the modern skyscraper in the 1880s." }
        },
        Quotes = new List<Quote>
        {
            new() { Text = "Government of the people, by the people, for the people, shall not perish from the earth.", Author = "Abraham Lincoln, Gettysburg Address, November 19, 1863" },
            new() { Text = "That's one small step for a man, one giant leap for mankind.", Author = "Neil Armstrong, on the Moon, July 20, 1969" },
            new() { Text = "I have not failed. I've just found 10,000 ways that won't work.", Author = "Thomas Edison" }
        },
        Videos = new List<Video>
        {
            new() { Title = "American Revolution: Road to Independence", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=american+revolution+documentary" },
            new() { Title = "Apollo 11: The Moon Landing", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=apollo+11+moon+landing+documentary" },
            new() { Title = "The Industrial Revolution in America", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=industrial+revolution+america+documentary" }
        },
        Sources = new List<Source>
        {
            new() { Label = "Wikipedia: United States", Url = "https://en.wikipedia.org/wiki/United_States" },
            new() { Label = "Britannica: Abraham Lincoln", Url = "https://www.britannica.com/biography/Abraham-Lincoln" },
            new() { Label = "NASA: Apollo 11", Url = "https://www.nasa.gov/mission_pages/apollo/apollo-11.html" },
            new() { Label = "Library of Congress: American History", Url = "https://www.loc.gov/topics/us-history/" }
        },
        QuizQuestions = new List<QuizQuestion>
        {
            new() { Question = "On what date did American astronauts first land on the Moon?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "July 4, 1969" }, new() { Text = "July 20, 1969" }, new() { Text = "September 12, 1962" }, new() { Text = "August 3, 1969" } } },
            new() { Question = "How many patents did Thomas Edison hold at the time of his death?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "Over 500" }, new() { Text = "Over 800" }, new() { Text = "Over 1,093" }, new() { Text = "Over 2,000" } } },
            new() { Question = "Which president issued the Emancipation Proclamation?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "Abraham Lincoln" }, new() { Text = "George Washington" }, new() { Text = "Ulysses S. Grant" }, new() { Text = "Andrew Johnson" } } },
            new() { Question = "On what date was the Declaration of Independence signed?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "July 4, 1775" }, new() { Text = "July 4, 1783" }, new() { Text = "July 4, 1776" }, new() { Text = "July 4, 1789" } } },
            new() { Question = "Which NASA mission first landed humans on the Moon?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Apollo 8" }, new() { Text = "Apollo 11" }, new() { Text = "Apollo 13" }, new() { Text = "Gemini 7" } } },
            new() { Question = "Who designed Washington D.C.'s street layout?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "Thomas Jefferson" }, new() { Text = "George Washington" }, new() { Text = "Benjamin Franklin" }, new() { Text = "Pierre Charles L'Enfant" } } },
            new() { Question = "Where did Edison build the world's first industrial research laboratory?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "Menlo Park, New Jersey" }, new() { Text = "Pittsburgh, Pennsylvania" }, new() { Text = "Chicago, Illinois" }, new() { Text = "New York City" } } },
            new() { Question = "How many terms did George Washington serve as President before voluntarily stepping down?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "One" }, new() { Text = "Two" }, new() { Text = "Three" }, new() { Text = "Four" } } },
            new() { Question = "In what year did Edison develop the first practical electric power distribution system in New York City?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "1876" }, new() { Text = "1879" }, new() { Text = "1882" }, new() { Text = "1895" } } },
            new() { Question = "The Gettysburg Address was delivered in how many words?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "150 words" }, new() { Text = "272 words" }, new() { Text = "400 words" }, new() { Text = "500 words" } } }
        }
    };

    /* era 4 — roman empire */
    var roman = new Era
    {
        Title = "Roman Empire", NodeLabel = "Roman Empire",
        Description = "Born from a city on the Tiber River, Rome grew over a thousand years into the most enduring empire the Western world has ever known — bequeathing to posterity a legacy of law, language, engineering, and governance that remains the bedrock of modern civilization.",
        Color = "#8b0000", BgColor = "#fff5f5",
        Stat1 = "80%", Stat2 = "92%", Stat3 = "98%",
        SectionIcon = "🏛️",
        SectionBody = "<p>The Roman Empire at its height stretched from Britain to Mesopotamia, governing over 70 million people under a sophisticated legal and administrative framework.</p><ul><li>Julius Caesar crossed the Rubicon in 49 BCE, triggering the end of the Republic.</li><li>Augustus inaugurated the Pax Romana — 200 years of relative peace.</li><li>Marcus Aurelius wrote the Meditations, a masterpiece of Stoic philosophy.</li><li>Hadrian's Wall stretched 73 miles across northern Britain.</li></ul>",
        Figures = new List<Figure>
        {
            new() { Name = "Julius Caesar", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1e/Julius_Caesar_Coustou_Louvre_MR1798.jpg/440px-Julius_Caesar_Coustou_Louvre_MR1798.jpg",
                Bio = "Gaius Julius Caesar (100–44 BCE) was the Roman general and statesman whose military campaigns transformed the Roman Republic into an empire. His conquest of Gaul, crossing of the Rubicon, and defeat of Pompey led to the civil war that ended the Republic. Assassinated on the Ides of March, 44 BCE." },
            new() { Name = "Augustus", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/eb/Statue-Augustus.jpg/440px-Statue-Augustus.jpg",
                Bio = "Gaius Octavius, known as Augustus (63 BCE–14 CE), was the first and arguably greatest Roman Emperor. He held supreme power while maintaining republican forms with extraordinary skill. His reign inaugurated the Pax Romana — 200 years of relative peace across the Mediterranean world." },
            new() { Name = "Marcus Aurelius", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/ec/MSR-ra-61-b-8.jpg/440px-MSR-ra-61-b-8.jpg",
                Bio = "Marcus Aurelius (121–180 CE) was the philosopher-emperor, last of the Five Good Emperors. Despite ruling during constant military pressure and devastating plague, he composed his Meditations — a private diary of Stoic philosophy that remains one of the most profound works of personal ethics ever written." },
            new() { Name = "Hadrian", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/32/Bust_Hadrian_Musei_Capitolini_MC817.jpg/440px-Bust_Hadrian_Musei_Capitolini_MC817.jpg",
                Bio = "Publius Aelius Hadrianus (76–138 CE) spent more than half his reign travelling the empire's provinces. Hadrian's Wall across northern Britain — 73 miles of stone fortification — is among the most impressive military engineering works of the ancient world. He also designed the Pantheon in Rome." }
        },
        Cities = new List<City>
        {
            new() { Name = "Rome", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/de/Colosseo_2020.jpg/640px-Colosseo_2020.jpg",
                Info = "The Eternal City grew to a population exceeding one million by the 1st century CE. Its architectural monuments — the Colosseum, the Forum, the Pantheon — represented the highest achievements of ancient engineering. Eleven aqueducts provided 300 gallons of fresh water per person per day." },
            new() { Name = "Carthage", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/99/Carthage_National_Museum.jpg/640px-Carthage_National_Museum.jpg",
                Info = "The ancient Phoenician city on the North African coast, destroyed in 146 BCE and refounded as a Roman colony. Carthage became the third-largest city in the Roman Empire and a major centre of grain production, trade, and Christian scholarship. Saint Augustine was born in Roman North Africa." },
            new() { Name = "Alexandria", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9b/Bibliotheca_Alexandrina_Panorama.jpg/640px-Bibliotheca_Alexandrina_Panorama.jpg",
                Info = "Founded by Alexander the Great in 331 BCE, Alexandria was the empire's second city and foremost centre of learning. Its Great Library attracted scholars from across the Mediterranean. The Pharos lighthouse was one of the Seven Wonders of the Ancient World." }
        },
        Quotes = new List<Quote>
        {
            new() { Text = "I came, I saw, I conquered.", Author = "Julius Caesar, after his swift victory at the Battle of Zela, 47 BCE" },
            new() { Text = "You have power over your mind, not outside events. Realize this, and you will find strength.", Author = "Marcus Aurelius, Meditations" },
            new() { Text = "Make haste slowly.", Author = "Augustus — his favourite maxim, counselling deliberate action" }
        },
        Videos = new List<Video>
        {
            new() { Title = "The Roman Empire: A History", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=roman+empire+history+documentary" },
            new() { Title = "Julius Caesar: The Rise and Fall", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=julius+caesar+documentary" },
            new() { Title = "Roman Engineering: Aqueducts and Roads", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=roman+engineering+aqueducts+roads+documentary" }
        },
        Sources = new List<Source>
        {
            new() { Label = "Wikipedia: Roman Empire", Url = "https://en.wikipedia.org/wiki/Roman_Empire" },
            new() { Label = "Britannica: Augustus", Url = "https://www.britannica.com/biography/Augustus-Roman-emperor" },
            new() { Label = "Marcus Aurelius Meditations (Penguin Classics)", Url = "https://www.penguinrandomhouse.com/books/545891/meditations-by-marcus-aurelius/" },
            new() { Label = "Cambridge Ancient History Vol. X", Url = "https://www.cambridge.org/core/books/cambridge-ancient-history/B4B3EF7B4FBB5B847F8E82A5E33CD57E" }
        },
        QuizQuestions = new List<QuizQuestion>
        {
            new() { Question = "What was the Pax Romana?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "The Roman peace treaty with Carthage" }, new() { Text = "A two-century period of relative peace under Roman rule" }, new() { Text = "The name of Rome's legal code" }, new() { Text = "The alliance between Rome and Greece" } } },
            new() { Question = "Which Roman emperor wrote the famous philosophical work 'Meditations'?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "Julius Caesar" }, new() { Text = "Augustus" }, new() { Text = "Hadrian" }, new() { Text = "Marcus Aurelius" } } },
            new() { Question = "How long is Hadrian's Wall across northern Britain?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "30 miles" }, new() { Text = "50 miles" }, new() { Text = "73 miles" }, new() { Text = "100 miles" } } },
            new() { Question = "Who was the first Roman Emperor?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "Augustus" }, new() { Text = "Julius Caesar" }, new() { Text = "Nero" }, new() { Text = "Tiberius" } } },
            new() { Question = "In what year did Julius Caesar cross the Rubicon?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "55 BCE" }, new() { Text = "49 BCE" }, new() { Text = "44 BCE" }, new() { Text = "31 BCE" } } },
            new() { Question = "Which ancient wonder stood in the Roman-controlled city of Alexandria?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "The Colossus of Rhodes" }, new() { Text = "The Hanging Gardens" }, new() { Text = "The Pharos lighthouse" }, new() { Text = "The Temple of Artemis" } } },
            new() { Question = "Julius Caesar was assassinated on what famous date?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "The Ides of March, 44 BCE" }, new() { Text = "The Ides of April, 44 BCE" }, new() { Text = "January 1, 44 BCE" }, new() { Text = "December 31, 45 BCE" } } },
            new() { Question = "How many aqueducts supplied water to ancient Rome?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "Five" }, new() { Text = "Eight" }, new() { Text = "Eleven" }, new() { Text = "Fourteen" } } },
            new() { Question = "What philosophy is Marcus Aurelius's Meditations associated with?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Epicureanism" }, new() { Text = "Stoicism" }, new() { Text = "Platonism" }, new() { Text = "Cynicism" } } },
            new() { Question = "The Battle of Actium in 31 BCE, which established Augustus's supremacy, was fought against whom?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "Pompey and the Senate" }, new() { Text = "Brutus and Cassius" }, new() { Text = "Carthage and Hannibal" }, new() { Text = "Mark Antony and Cleopatra" } } }
        }
    };

    /* era 5 — mongol empire */
    var mongol = new Era
    {
        Title = "Mongol Empire", NodeLabel = "Mongol Empire",
        Description = "The largest contiguous land empire in history, forged in a single generation by a nomadic warrior of exceptional genius, the Mongol Empire connected East and West, devastated settled civilizations, and paradoxically enabled an era of unprecedented Eurasian connectivity.",
        Color = "#7b5e3a", BgColor = "#fdf6ec",
        Stat1 = "65%", Stat2 = "98%", Stat3 = "100%",
        SectionIcon = "🏹",
        SectionBody = "<p>The Mongol Empire at its peak covered 24 million square kilometres — roughly 16% of the Earth's total land area — making it the largest contiguous empire in history.</p><ul><li>Genghis Khan united the Mongol tribes by 1206.</li><li>Mongol armies reached the Adriatic Sea in 1241.</li><li>The Pax Mongolica enabled unprecedented Silk Road trade.</li><li>Kublai Khan established the Yuan Dynasty in China.</li></ul>",
        Figures = new List<Figure>
        {
            new() { Name = "Genghis Khan", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/ea/YuanEmperorAlbumGenghisPortrait.jpg/440px-YuanEmperorAlbumGenghisPortrait.jpg",
                Bio = "Born Temüjin around 1162 CE, Genghis Khan united the Mongol tribes by 1206 through force of personality, military genius, and loyalty-building across tribal divisions. In the next twenty years, his armies conquered from China to Persia, creating the framework of the largest land empire in history." },
            new() { Name = "Kublai Khan", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/KublaiKhan.jpg/440px-KublaiKhan.jpg",
                Bio = "Kublai Khan (1215–1294), grandson of Genghis, completed the conquest of China and established the Yuan Dynasty in 1271. He maintained a splendid court at Khanbaliq (modern Beijing) that welcomed Marco Polo, who spent 17 years in his service." },
            new() { Name = "Ögedei Khan", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e9/%C3%96gedei_Khan.jpg/440px-%C3%96gedei_Khan.jpg",
                Bio = "Ögedei Khan (1186–1241), third son of Genghis and his chosen successor, oversaw the most dramatic phase of Mongol expansion in the West. Under his command, Mongol armies devastated Poland and Hungary and reached the Adriatic Sea in 1241 — only his sudden death caused the western armies to withdraw." },
            new() { Name = "Timur", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5a/Timur_reconstruction03.jpg/440px-Timur_reconstruction03.jpg",
                Bio = "Timur, also known as Tamerlane (1336–1405), claimed descent from Genghis Khan. From his capital at Samarkand, he launched campaigns that devastated Delhi, Baghdad, Damascus, and Ankara. Yet he was simultaneously a passionate patron of art and architecture, adorning Samarkand with breathtaking mosques and madrasas." }
        },
        Cities = new List<City>
        {
            new() { Name = "Karakorum", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5c/Karakorum_Erdene_Zu.jpg/640px-Karakorum_Erdene_Zu.jpg",
                Info = "Founded by Ögedei Khan around 1220 CE as the imperial capital, Karakorum was a cosmopolitan city with Chinese craftsmen, Persian merchants, European envoys, and Buddhist, Christian, and Muslim places of worship existing side by side." },
            new() { Name = "Samarkand", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/af/Samarkand_Registan_Mosque.JPG/640px-Samarkand_Registan_Mosque.JPG",
                Info = "One of the oldest cities in Central Asia, Samarkand's position at the crossroads of the Silk Road made it the jewel of Mongol Central Asia. Under Timur in the late 14th century, it became one of the most magnificent cities in the Islamic world, adorned with the Registan square and the Bibi-Khanym Mosque." },
            new() { Name = "Khanbaliq", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/10/Forbidden_City_Beijing_Shenwumen_Gate.jpg/640px-Forbidden_City_Beijing_Shenwumen_Gate.jpg",
                Info = "Established by Kublai Khan as the capital of the Yuan Dynasty on the site of modern Beijing, Khanbaliq was designed on a magnificent scale. Marco Polo, who spent years at Kublai's court, described it as surpassing all other cities in splendour." }
        },
        Quotes = new List<Quote>
        {
            new() { Text = "The strength of a wall is neither greater nor less than the courage of the men who defend it.", Author = "Genghis Khan" },
            new() { Text = "If you're afraid — don't do it. If you're doing it — don't be afraid.", Author = "Genghis Khan" },
            new() { Text = "It is not sufficient that I succeed — all others must fail.", Author = "Attributed to Genghis Khan" }
        },
        Videos = new List<Video>
        {
            new() { Title = "Genghis Khan and the Mongol Empire", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=genghis+khan+mongol+empire+documentary" },
            new() { Title = "The Pax Mongolica: Silk Road Under Mongol Rule", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=pax+mongolica+silk+road+documentary" },
            new() { Title = "Marco Polo and Kublai Khan", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=marco+polo+kublai+khan+documentary" }
        },
        Sources = new List<Source>
        {
            new() { Label = "Wikipedia: Mongol Empire", Url = "https://en.wikipedia.org/wiki/Mongol_Empire" },
            new() { Label = "Britannica: Genghis Khan", Url = "https://www.britannica.com/biography/Genghis-Khan" },
            new() { Label = "The Secret History of the Mongols (Penguin Classics)", Url = "https://www.penguinrandomhouse.com/books/289399/the-secret-history-of-the-mongols-by-anonymous/" },
            new() { Label = "Jack Weatherford: Genghis Khan and the Making of the Modern World", Url = "https://www.google.com/search?q=Jack+Weatherford+Genghis+Khan+Making+Modern+World" }
        },
        QuizQuestions = new List<QuizQuestion>
        {
            new() { Question = "What was the Pax Mongolica?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "A peace treaty between Mongol clans" }, new() { Text = "An era of Eurasian trade and connectivity under Mongol protection" }, new() { Text = "The Mongol legal code" }, new() { Text = "Genghis Khan's final campaign" } } },
            new() { Question = "At its peak, approximately what percentage of the Earth's total land area did the Mongol Empire cover?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "6%" }, new() { Text = "10%" }, new() { Text = "16%" }, new() { Text = "24%" } } },
            new() { Question = "What was Genghis Khan's birth name?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "Temüjin" }, new() { Text = "Börte" }, new() { Text = "Subutai" }, new() { Text = "Jochi" } } },
            new() { Question = "Kublai Khan established which dynasty in China?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "Ming Dynasty" }, new() { Text = "Tang Dynasty" }, new() { Text = "Song Dynasty" }, new() { Text = "Yuan Dynasty" } } },
            new() { Question = "Which European traveller spent 17 years at Kublai Khan's court?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Ibn Battuta" }, new() { Text = "Marco Polo" }, new() { Text = "William of Rubruck" }, new() { Text = "Rabban Sauma" } } },
            new() { Question = "What event in 1241 halted the Mongol advance into Western Europe?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "Defeat at the Battle of Legnica" }, new() { Text = "A devastating plague in the Mongol army" }, new() { Text = "The sudden death of Ögedei Khan" }, new() { Text = "A military alliance of European kingdoms" } } },
            new() { Question = "Where was the imperial capital of the Mongol Empire, founded by Ögedei Khan?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "Karakorum" }, new() { Text = "Samarkand" }, new() { Text = "Khanbaliq" }, new() { Text = "Bukhara" } } },
            new() { Question = "Timur (Tamerlane) made which city his spectacular capital?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "Tabriz" }, new() { Text = "Samarkand" }, new() { Text = "Delhi" }, new() { Text = "Herat" } } },
            new() { Question = "In what year did Genghis Khan officially unite the Mongol tribes?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "1189" }, new() { Text = "1200" }, new() { Text = "1206" }, new() { Text = "1215" } } },
            new() { Question = "Which famous Mongol general led the armies that swept into Poland and Hungary in 1241?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "Jebe" }, new() { Text = "Batu Khan" }, new() { Text = "Tolui" }, new() { Text = "Subutai" } } }
        }
    };

    /* era 6 — british empire */
    var british = new Era
    {
        Title = "British Empire", NodeLabel = "British Empire",
        Description = "The largest empire in history by territorial extent, the British Empire governed nearly a quarter of humanity at its peak, industrialized the globe, reshaped the political map of every continent, and left an enduring — and deeply contested — legacy.",
        Color = "#1d3557", BgColor = "#eaf0f8",
        Stat1 = "88%", Stat2 = "96%", Stat3 = "100%",
        SectionIcon = "🌍",
        SectionBody = "<p>At its Victorian peak, the British Empire covered 24% of the world's land surface and governed nearly 458 million people — a quarter of the world's population.</p><ul><li>Queen Victoria was proclaimed Empress of India in 1876.</li><li>The Bank of England, founded 1694, made London the centre of global finance.</li><li>Isaac Newton's Principia Mathematica (1687) defined physics for 200 years.</li><li>Darwin's On the Origin of Species (1859) founded modern biology.</li></ul>",
        Figures = new List<Figure>
        {
            new() { Name = "Queen Victoria", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e3/Queen_Victoria_by_Bassano.jpg/440px-Queen_Victoria_by_Bassano.jpg",
                Bio = "Queen Victoria (1819–1901) reigned for 63 years and became the symbolic embodiment of empire itself. Proclaimed Empress of India in 1876, during her reign the empire reached its greatest territorial extent. Her devoted relationship with her subjects made her a figure of popular adoration unprecedented in British history." },
            new() { Name = "Winston Churchill", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bc/Sir_Winston_Churchill_-_19086236948.jpg/440px-Sir_Winston_Churchill_-_19086236948.jpg",
                Bio = "Winston Churchill (1874–1965) served as British Prime Minister during World War II (1940–1945). His combination of strategic vision, rhetorical brilliance, and sheer personal will did more than any other individual to prevent Nazi Germany from dominating Europe. Awarded the Nobel Prize in Literature in 1953." },
            new() { Name = "Isaac Newton", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/39/GodfreyKneller-IsaacNewton-1689.jpg/440px-GodfreyKneller-IsaacNewton-1689.jpg",
                Bio = "Sir Isaac Newton (1643–1727) was the greatest scientist in British history. His Principia Mathematica (1687) formulated the laws of motion and universal gravitation. He also invented calculus, conducted pioneering experiments in optics, and his work defined the scientific method for two centuries." },
            new() { Name = "Charles Darwin", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2e/Charles_Darwin_seated_crop.jpg/440px-Charles_Darwin_seated_crop.jpg",
                Bio = "Charles Darwin (1809–1882) was the naturalist whose theory of evolution by natural selection — published in On the Origin of Species in 1859 — is the foundational principle of modern biology. His five-year voyage on HMS Beagle provided the empirical basis for a theory that overturned centuries of orthodoxy." }
        },
        Cities = new List<City>
        {
            new() { Name = "London", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/67/London_Skyline_%28125508655%29.jpeg/640px-London_Skyline_%28125508655%29.jpeg",
                Info = "The greatest city of the 19th century, London was simultaneously the political capital of the British Empire, the financial centre of the global economy, and a metropolis of extraordinary cultural richness. The Port of London was the busiest in the world, handling goods from every corner of the empire." },
            new() { Name = "Calcutta", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8b/Howrah_Bridge_Kolkata.jpg/640px-Howrah_Bridge_Kolkata.jpg",
                Info = "Founded as a trading post by the East India Company in 1690, Calcutta grew into the administrative capital of British India. For two centuries it was the seat of the Viceroy, a hub of Bengal's textile industries, and a centre of Indian intellectual renaissance including figures like Rabindranath Tagore." },
            new() { Name = "Cape Town", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1b/Cape_Town_from_Table_Mountain.jpg/640px-Cape_Town_from_Table_Mountain.jpg",
                Info = "Occupied by the British in 1806, Cape Town became the strategic guardian of the sea route to India. The discovery of diamonds (1867) and gold (1886) transformed southern Africa into the most economically valuable region of the empire, triggering the catastrophic Anglo-Boer War (1899–1902)." }
        },
        Quotes = new List<Quote>
        {
            new() { Text = "We shall fight on the beaches, we shall fight on the landing grounds, we shall fight in the fields and in the streets, we shall never surrender.", Author = "Winston Churchill, House of Commons, June 4, 1940" },
            new() { Text = "If I have seen further, it is by standing on the shoulders of giants.", Author = "Isaac Newton, letter to Robert Hooke, February 5, 1675" },
            new() { Text = "It is not the strongest of the species that survives, nor the most intelligent; it is the one most adaptable to change.", Author = "Attributed to Charles Darwin, paraphrasing On the Origin of Species" }
        },
        Videos = new List<Video>
        {
            new() { Title = "The British Empire: How It Happened", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=british+empire+documentary" },
            new() { Title = "Winston Churchill: The Darkest Hour", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=winston+churchill+documentary" },
            new() { Title = "Isaac Newton: The Last Magician", Channel = "YouTube Research", Url = "https://www.youtube.com/results?search_query=isaac+newton+documentary+bbc" }
        },
        Sources = new List<Source>
        {
            new() { Label = "Wikipedia: British Empire", Url = "https://en.wikipedia.org/wiki/British_Empire" },
            new() { Label = "Britannica: Queen Victoria", Url = "https://www.britannica.com/biography/Victoria-queen-of-United-Kingdom" },
            new() { Label = "Cambridge History of the British Empire Vol. III", Url = "https://www.cambridge.org/core/books/cambridge-history-of-the-british-empire/85CFDC7F741B5B9C28A3D5B5A1DBFF12" },
            new() { Label = "Niall Ferguson: Empire (Basic Books)", Url = "https://www.google.com/search?q=Niall+Ferguson+Empire+Basic+Books" }
        },
        QuizQuestions = new List<QuizQuestion>
        {
            new() { Question = "At its territorial peak, roughly what percentage of the world's land did the British Empire govern?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "10%" }, new() { Text = "18%" }, new() { Text = "24%" }, new() { Text = "33%" } } },
            new() { Question = "Which British scientist formulated the laws of motion and universal gravitation?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "Charles Darwin" }, new() { Text = "Michael Faraday" }, new() { Text = "Isaac Newton" }, new() { Text = "James Watt" } } },
            new() { Question = "In what year was Queen Victoria proclaimed Empress of India?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "1858" }, new() { Text = "1876" }, new() { Text = "1887" }, new() { Text = "1901" } } },
            new() { Question = "Charles Darwin published 'On the Origin of Species' in which year?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "1831" }, new() { Text = "1845" }, new() { Text = "1851" }, new() { Text = "1859" } } },
            new() { Question = "Which ship did Charles Darwin sail on during his famous five-year voyage?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "HMS Beagle" }, new() { Text = "HMS Victory" }, new() { Text = "HMS Endeavour" }, new() { Text = "HMS Bounty" } } },
            new() { Question = "In what year did Winston Churchill win the Nobel Prize in Literature?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "1945" }, new() { Text = "1950" }, new() { Text = "1953" }, new() { Text = "1958" } } },
            new() { Question = "Newton's Principia Mathematica was published in which year?",
                CorrectIndex = 1, Options = new List<QuizOption> { new() { Text = "1666" }, new() { Text = "1687" }, new() { Text = "1704" }, new() { Text = "1727" } } },
            new() { Question = "The Anglo-Boer War in southern Africa took place between which years?",
                CorrectIndex = 3, Options = new List<QuizOption> { new() { Text = "1867–1870" }, new() { Text = "1879–1882" }, new() { Text = "1890–1894" }, new() { Text = "1899–1902" } } },
            new() { Question = "How long did Queen Victoria reign?",
                CorrectIndex = 0, Options = new List<QuizOption> { new() { Text = "63 years" }, new() { Text = "50 years" }, new() { Text = "45 years" }, new() { Text = "70 years" } } },
            new() { Question = "Which famous speech did Churchill deliver on June 4, 1940, containing the words 'we shall never surrender'?",
                CorrectIndex = 2, Options = new List<QuizOption> { new() { Text = "The Iron Curtain speech" }, new() { Text = "Their Finest Hour" }, new() { Text = "We Shall Fight on the Beaches" }, new() { Text = "Blood, Toil, Tears and Sweat" } } }
        }
    };

    ctx.Eras.AddRange(abbasid, ottoman, american, roman, mongol, british);
    ctx.SaveChanges();
}

/* quiz patcher — runs when eras already exist but quiz questions are missing */
static void EnsureQuizQuestions(HistoryContext ctx)
{
    /* map era title to the 10 questions it should have */
    var allQuestions = new Dictionary<string, List<(string Q, int Correct, string[] Opts)>>
    {
        ["The Abbasid Caliphate"] = new()
        {
            ("What was the name of the great academic institution founded in Baghdad during the Abbasid Caliphate?", 1, new[]{"Al-Azhar University","The House of Wisdom","The Grand Academy","The School of Baghdad"}),
            ("Which Abbasid scholar is known as the 'father of algebra' for his mathematical treatise?", 2, new[]{"Al-Kindi","Ibn Sina","Al-Khwarizmi","Al-Biruni"}),
            ("In what year was the Abbasid Caliphate founded?", 1, new[]{"622 CE","750 CE","830 CE","900 CE"}),
            ("Who founded Baghdad in 762 CE?", 2, new[]{"Harun al-Rashid","Al-Kindi","Caliph Al-Mansur","Al-Khwarizmi"}),
            ("What does the word 'algorithm' derive from?", 0, new[]{"The Latinized name of Al-Khwarizmi","An Arabic word for calculation","A Greek mathematical term","The name of a Baghdad school"}),
            ("Which Western name is Ibn Sina known by?", 3, new[]{"Averroes","Alhazen","Albumasar","Avicenna"}),
            ("What was the Translation Movement in Abbasid Baghdad?", 1, new[]{"A program to translate the Quran into all languages","A systematic effort to translate Greek, Persian, and Indian texts into Arabic","A school that taught foreign languages to diplomats","A movement to spread Arabic literature to Europe"}),
            ("Which empire did the Abbasids overthrow to seize the caliphate?", 0, new[]{"The Umayyad Caliphate","The Sasanian Empire","The Byzantine Empire","The Fatimid Caliphate"}),
            ("In what year did the Mongol invasion destroy Baghdad and end the Abbasid Caliphate?", 2, new[]{"1187 CE","1204 CE","1258 CE","1301 CE"}),
            ("Al-Kindi is credited as the first philosopher in which tradition?", 1, new[]{"Persian philosophical tradition","Arabic philosophical tradition","Ottoman philosophical tradition","Byzantine philosophical tradition"}),
        },
        ["Ottoman Empire"] = new()
        {
            ("In what year did Ottoman Sultan Mehmed II conquer Constantinople?", 1, new[]{"1389","1453","1517","1529"}),
            ("Which Ottoman architect designed both the Süleymaniye Mosque and the Selimiye Mosque?", 1, new[]{"Sinan Bey","Mimar Sinan","Hayreddin","Mehmed Aga"}),
            ("How long did Suleiman I reign as Ottoman Sultan?", 2, new[]{"20 years","35 years","46 years","60 years"}),
            ("By what nickname was Suleiman I known in Western Europe?", 0, new[]{"The Magnificent","The Conqueror","The Lawgiver","The Builder"}),
            ("Which city served as the first major capital of the Ottoman Empire before Istanbul?", 3, new[]{"Ankara","Edirne","Konya","Bursa"}),
            ("What was the Kanun, as associated with Suleiman I?", 1, new[]{"The name of Suleiman's palace","A codification of Ottoman secular law","The Ottoman navy's battle flag","A famous Ottoman musical instrument"}),
            ("Hayreddin Barbarossa secured Ottoman dominance in the Mediterranean at which battle?", 2, new[]{"Battle of Lepanto","Battle of Mohács","Battle of Preveza","Battle of Vienna"}),
            ("In which year did the Ottoman Empire officially end?", 3, new[]{"1908","1912","1918","1922"}),
            ("How many languages was Mehmed II reputed to speak?", 1, new[]{"Three","Six","Eight","Ten"}),
            ("The Ottoman conquest of Egypt in 1517 brought which city under Ottoman rule?", 0, new[]{"Cairo","Alexandria","Mecca","Jerusalem"}),
        },
        ["American Era"] = new()
        {
            ("On what date did American astronauts first land on the Moon?", 1, new[]{"July 4, 1969","July 20, 1969","September 12, 1962","August 3, 1969"}),
            ("How many patents did Thomas Edison hold at the time of his death?", 2, new[]{"Over 500","Over 800","Over 1,093","Over 2,000"}),
            ("Which president issued the Emancipation Proclamation?", 0, new[]{"Abraham Lincoln","George Washington","Ulysses S. Grant","Andrew Johnson"}),
            ("On what date was the Declaration of Independence signed?", 2, new[]{"July 4, 1775","July 4, 1783","July 4, 1776","July 4, 1789"}),
            ("Which NASA mission first landed humans on the Moon?", 1, new[]{"Apollo 8","Apollo 11","Apollo 13","Gemini 7"}),
            ("Who designed Washington D.C.'s street layout?", 3, new[]{"Thomas Jefferson","George Washington","Benjamin Franklin","Pierre Charles L'Enfant"}),
            ("Where did Edison build the world's first industrial research laboratory?", 0, new[]{"Menlo Park, New Jersey","Pittsburgh, Pennsylvania","Chicago, Illinois","New York City"}),
            ("How many terms did George Washington serve as President before voluntarily stepping down?", 1, new[]{"One","Two","Three","Four"}),
            ("In what year did Edison develop the first practical electric power distribution system in New York City?", 2, new[]{"1876","1879","1882","1895"}),
            ("The Gettysburg Address was delivered in how many words?", 1, new[]{"150 words","272 words","400 words","500 words"}),
        },
        ["Roman Empire"] = new()
        {
            ("What was the Pax Romana?", 1, new[]{"The Roman peace treaty with Carthage","A two-century period of relative peace under Roman rule","The name of Rome's legal code","The alliance between Rome and Greece"}),
            ("Which Roman emperor wrote the famous philosophical work 'Meditations'?", 3, new[]{"Julius Caesar","Augustus","Hadrian","Marcus Aurelius"}),
            ("How long is Hadrian's Wall across northern Britain?", 2, new[]{"30 miles","50 miles","73 miles","100 miles"}),
            ("Who was the first Roman Emperor?", 0, new[]{"Augustus","Julius Caesar","Nero","Tiberius"}),
            ("In what year did Julius Caesar cross the Rubicon?", 1, new[]{"55 BCE","49 BCE","44 BCE","31 BCE"}),
            ("Which ancient wonder stood in the Roman-controlled city of Alexandria?", 2, new[]{"The Colossus of Rhodes","The Hanging Gardens","The Pharos lighthouse","The Temple of Artemis"}),
            ("Julius Caesar was assassinated on what famous date?", 0, new[]{"The Ides of March, 44 BCE","The Ides of April, 44 BCE","January 1, 44 BCE","December 31, 45 BCE"}),
            ("How many aqueducts supplied water to ancient Rome?", 2, new[]{"Five","Eight","Eleven","Fourteen"}),
            ("What philosophy is Marcus Aurelius's Meditations associated with?", 1, new[]{"Epicureanism","Stoicism","Platonism","Cynicism"}),
            ("The Battle of Actium in 31 BCE was fought against whom?", 3, new[]{"Pompey and the Senate","Brutus and Cassius","Carthage and Hannibal","Mark Antony and Cleopatra"}),
        },
        ["Mongol Empire"] = new()
        {
            ("What was the Pax Mongolica?", 1, new[]{"A peace treaty between Mongol clans","An era of Eurasian trade and connectivity under Mongol protection","The Mongol legal code","Genghis Khan's final campaign"}),
            ("At its peak, approximately what percentage of the Earth's total land area did the Mongol Empire cover?", 2, new[]{"6%","10%","16%","24%"}),
            ("What was Genghis Khan's birth name?", 0, new[]{"Temüjin","Börte","Subutai","Jochi"}),
            ("Kublai Khan established which dynasty in China?", 3, new[]{"Ming Dynasty","Tang Dynasty","Song Dynasty","Yuan Dynasty"}),
            ("Which European traveller spent 17 years at Kublai Khan's court?", 1, new[]{"Ibn Battuta","Marco Polo","William of Rubruck","Rabban Sauma"}),
            ("What event in 1241 halted the Mongol advance into Western Europe?", 2, new[]{"Defeat at the Battle of Legnica","A devastating plague in the Mongol army","The sudden death of Ögedei Khan","A military alliance of European kingdoms"}),
            ("Where was the imperial capital of the Mongol Empire, founded by Ögedei Khan?", 0, new[]{"Karakorum","Samarkand","Khanbaliq","Bukhara"}),
            ("Timur (Tamerlane) made which city his spectacular capital?", 1, new[]{"Tabriz","Samarkand","Delhi","Herat"}),
            ("In what year did Genghis Khan officially unite the Mongol tribes?", 2, new[]{"1189","1200","1206","1215"}),
            ("Which famous Mongol general led the armies that swept into Poland and Hungary in 1241?", 3, new[]{"Jebe","Batu Khan","Tolui","Subutai"}),
        },
        ["British Empire"] = new()
        {
            ("At its territorial peak, roughly what percentage of the world's land did the British Empire govern?", 2, new[]{"10%","18%","24%","33%"}),
            ("Which British scientist formulated the laws of motion and universal gravitation?", 2, new[]{"Charles Darwin","Michael Faraday","Isaac Newton","James Watt"}),
            ("In what year was Queen Victoria proclaimed Empress of India?", 1, new[]{"1858","1876","1887","1901"}),
            ("Charles Darwin published 'On the Origin of Species' in which year?", 3, new[]{"1831","1845","1851","1859"}),
            ("Which ship did Charles Darwin sail on during his famous five-year voyage?", 0, new[]{"HMS Beagle","HMS Victory","HMS Endeavour","HMS Bounty"}),
            ("In what year did Winston Churchill win the Nobel Prize in Literature?", 2, new[]{"1945","1950","1953","1958"}),
            ("Newton's Principia Mathematica was published in which year?", 1, new[]{"1666","1687","1704","1727"}),
            ("The Anglo-Boer War in southern Africa took place between which years?", 3, new[]{"1867–1870","1879–1882","1890–1894","1899–1902"}),
            ("How long did Queen Victoria reign?", 0, new[]{"63 years","50 years","45 years","70 years"}),
            ("Which famous speech did Churchill deliver on June 4, 1940?", 2, new[]{"The Iron Curtain speech","Their Finest Hour","We Shall Fight on the Beaches","Blood, Toil, Tears and Sweat"}),
        },
    };

    /* load eras with existing questions and check if any are missing */
    var eras = ctx.Eras.Include(e => e.QuizQuestions).ThenInclude(q => q.Options).ToList();
    bool changed = false;

    foreach (var era in eras)
    {
        if (!allQuestions.ContainsKey(era.Title)) continue;
        if (era.QuizQuestions.Count >= 10) continue;   // already has enough questions

        /* remove whatever partial questions exist, start fresh for this era */
        ctx.QuizQuestions.RemoveRange(era.QuizQuestions);

        foreach (var (q, correct, opts) in allQuestions[era.Title])
        {
            var question = new QuizQuestion
            {
                EraId        = era.Id,
                Question     = q,
                CorrectIndex = correct,
                Options      = opts.Select(o => new QuizOption { Text = o }).ToList()
            };
            ctx.QuizQuestions.Add(question);
        }
        changed = true;
    }

    if (changed) ctx.SaveChanges();
}