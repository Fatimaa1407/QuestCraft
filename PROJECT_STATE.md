# QuestCraft — Layihə Statusu

> Son yeniləmə: 2026-07-09 (davam edir). Bu fayl sessiyalar arasında davamlılıq üçün saxlanılır — hər böyük mərhələ bitəndə yenilənir.

> **Qeyd (dil)**: Bu istifadəçi ilə həmişə Azərbaycan dilində danışılmalıdır (`memory/feedback_language.md`-də saxlanılıb).

## Repo / Mühit

- **Path**: `C:\Users\User\OneDrive\Desktop\Code Layihe` — GitHub: `https://github.com/Fatimaa1407/QuestCraft.git` (bu qovluq Desktop-dakı böyük repo-dan ayrı, müstəqil git repo-dur)
- **Backend**: `.NET 10`, Clean Architecture (`backend/src/QuestCraft.{Domain,Application,Infrastructure,API}` + `backend/tests/*`)
- **Frontend**: React 19 + TypeScript + Vite, `frontend/` — dev port **5176** (strictPort, `vite.config.ts`-də sabitlənib)
- **Database**: SQL Server Express, `.\SQLEXPRESS`, database adı `QuestCraftDb` (Trusted Connection)
- **Admin login**: `admin@questcraft.local` / `Admin@12345`
- **Architecture sənədi**: `docs/ARCHITECTURE.md` (ilkin tam sistem dizaynı, bəzi qərarlar sonradan real tətbiqdə dəyişib — bax "Plandan kənarlaşmalar")

### Server işə salma
```
# Backend (Development mühiti avtomatik migration+seed işlədir)
cd backend/src/QuestCraft.API
ASPNETCORE_ENVIRONMENT=Development dotnet run --urls "https://localhost:5299"

# Frontend
cd frontend
npm run dev   # http://localhost:5176
```
CORS `backend/src/QuestCraft.API/appsettings.Development.json` → `Cors:AllowedOrigins` = `["http://localhost:5176"]`.

## Git vəziyyəti

Son commit: `a93410a Add marketplace`. Bundan sonrakı hər şey (Excel/Audit Log backend modulu + bütün frontend işi: infra + Auth UI + rəng sistemi + User FirstName/LastName) **hələ commit edilməyib** — istifadəçi "bu gün üçün commit atma" demişdi. Növbəti dəfə commit/push haqqında aydın təlimat gözlənilir.

## Backend — TAM BİTİB (bütün planlaşdırılan modullar)

1. Layihə skeleti, 2. Database qatı (33+ entity), 3. Auth (JWT+Refresh+BCrypt), 4. Admin CRUD (+soft-delete/restore),
5. Code Execution + Submission, 6. Gamification (Streak/Achievement/DailyQuest/Leaderboard/Notifications),
7. Quiz sistemi, 8. Marketplace (+transaction/rollback), 9. Excel Import/Export + Audit Log.

Hamısı curl/Playwright ilə uc-uca test edilib. Detallar üçün əvvəlki commit mesajlarına və `docs/ARCHITECTURE.md`-ə bax.

### Bug fix-lər
- **Cookie SameSite**: `SameSite=None` (Secure=true) — frontend(http)/backend(https) fərqli scheme olduğu üçün Strict cookie-ni bloklayırdı.
- **User.FirstName/LastName** əlavə edildi, dev DB sıfırdan yaradıldı (yalnız test data var idi).
- **[Kritik] Register-də JWT UserId=0** (`RegisterCommandHandler.cs`) — `GenerateAccessToken(user)` `SaveChangesAsync()`-dən **əvvəl** çağırılırdı, ona görə `user.Id` hələ DB-nin verdiyi IDENTITY dəyərini almamış (default 0) olurdu. Nəticə: hər yeni qeydiyyatdan keçən istifadəçinin ilk sessiya token-i `sub=0` daşıyırdı — bu, o sessiyada edilən istənilən yazma əməliyyatını (challenge submit və s.) FK constraint xətası ilə çökdürürdü. **Fix**: `SaveChangesAsync()` əvvəlcə çağırılır (User.Id populate olunur), sonra token generasiya olunur, sonra RefreshToken üçün ikinci `SaveChangesAsync()`. İstifadəçi tərəfindən tapılıb ("1 challenge-i 3 dəfə yazıb mükafat topladı" tapılanda əlaqəli araşdırma zamanı aşkarlandı), curl+Playwright ilə (təzə qeydiyyat → dərhal submit, uğurlu) təsdiqləndi.
- **[Kiçik, UI] Daily Quest claim-dən sonra header-dəki Coin/XP dərhal yenilənmirdi** — `ClaimDailyQuestRewardCommand` DB-də Coin-i düzgün artırırdı (backend özü düzgün idi), amma cavab DTO-su (`DailyQuestDto`) yalnız tapşırığın mükafat MİQDARINI daşıyırdı, istifadəçinin YENİ TOTAL balansını yox — frontend-də də `claimMutation.onSuccess` yalnız quest siyahısını invalidasiya edirdi, `authStore`-u yeniləmirdi (səhifə yenilənəndə/yenidən login olanda düzgün görünürdü, ona görə "sonradan düzgün gördüm" — canlı UI gecikməsi idi, data itkisi deyil). **Fix**: backend-ə `ClaimDailyQuestResultDto(Quest, TotalXp, TotalCoins, Level)` əlavə edildi, frontend `claimMutation.onSuccess`-də bu dəyərlərlə `authStore.updateUser(...)` çağırılır (Shop-dakı satınalma pattern-i ilə eyni məntiq). Fresh test istifadəçisi ilə təsdiqləndi: claim-dən dərhal sonra, səhifə yenilənmədən, header 40XP/0Coin→70XP/10Coin göstərdi.
- **[Orta] Daily Quest "3 Challenge həll et" progress-i təkrar submit-də artırdı** (`SubmitChallengeCommand.cs` sətir ~158) — `DailyQuestService.UpdateProgressAsync(SolveChallenge, 1)` çağırışı yalnız `execution.Verdict == Accepted` şərtinə bağlı idi, `isFirstAcceptedSolve` (artıq həll edilib yoxlaması, XP/Coin-in təkrar verilməməsi üçün istifadə olunan eyni bayraq) yoxlanmırdı. İstifadəçi bunu istifadəçi kimi tapıb bildirdi: eyni challenge-i 3 dəfə submit edib "3 challenge həll et" tapşırığını saxta yolla tamamlamışdı. **Fix**: çağırış `isFirstAcceptedSolve` şərtinə bağlandı (streak/activityLog məntiqi toxunulmadı, onlar hər Accepted-də düzgün işləməlidir). Fresh test user ilə təsdiqləndi: eyni challenge 3 dəfə submit → progress 0→1-də qalır, artmır.

### Qeyd
- `GET /api/users/me` kimi ayrıca "current user profile" endpoint-i **yoxdur** — UserDto login/register/refresh cavablarında gəlir, frontend authStore-da saxlanılır. Lazım olsa gələcəkdə əlavə edilə bilər (məs. profil səhifəsi üçün).

## Frontend

### Tamamlanıb
- **Core infra**: Axios+JWT/refresh interceptor, Zustand authStore+themeStore, TanStack Query, React Router (`ProtectedRoute`, `AppLayout`)
- **i18n**: AZ/EN (`i18next`/`react-i18next`)
- **Dark/Light tema**: Tailwind class-based, localStorage-da saxlanılır
- **Login/Register səhifələri** — bir neçə iterasiyadan sonra son vəziyyət:
  - Təmiz 50/50 split (sol hero / sağ forma), forma dikey mərkəzləşdirilib
  - **Rəng sistemi**: tünd navy fon (`slate-950`), **indigo** əsas marka rəngi, **cyan** aksent, bənövşəyi yalnız çox həssas fon "glow"-u kimi (GitHub/Linear/Vercel/Raycast texniki estetikası — "game-like" bənövşəyi deyil)
  - Sol paneldə: tagline+features + **3 kart bir diaqonal kaskadda** (Editor kartı dominant/böyük, XP və Leaderboard kartları kiçik, aşağı-sağa doğru yüngül overlap ilə) — istifadəçi bu düzülüşü bəyənib, **toxunulmasın**
  - Sağ paneldə: kompakt (`max-w-[21rem]`) elevated-surface forma kartı, güclü kontrast (dark: `bg-slate-800/95` üzərində `bg-slate-950` fon)
  - Register: Ad, Soyad, Username, Email, Şifrə (canlı checklist: 8+ simvol/böyük/kiçik/rəqəm), Şifrəni təsdiqlə — hamısı göz-ikonlu show/hide
  - Fon: `CodeBackdrop` (üzən kod snippet-ləri, indigo/cyan glow-lar) + `ParticleField` (çox həssas yanıb-sönən nöqtələr, boş sahələri doldurur)
  - Playwright ilə brauzer testi: unauthenticated redirect, login, logout, register, **reload-dan sonra session qalır** (cookie fix işləyir), hər iki tema, hər iki dil

- **Sol Sidebar naviqasiya** (`components/layout/Sidebar.tsx`) — köhnə üfüqi top-nav əvəzinə: Dashboard/Challenges/Practice/Leaderboard/Achievements/Shop/Profile. Hələ qurulmamış səhifələr (`ComingSoonPage`) tezliklə hazır olacaq placeholder-ları göstərir, dizayn dilinə uyğun.
- **Dashboard** — real data: stat kartları (Level/XP/Coins/Rol), Daily Quests widget (proqres + claim düyməsi), son submission-lar (verdict badge), son quiz nəticələri (TanStack Query ilə)
- **Challenges** — `ChallengesListPage` (axtarış + kateqoriya/çətinlik filtri + pagination) və `ChallengeDetailPage` (təsvir/constraint/nümunə giriş-çıxış/hint-unlock + Monaco Editor (C#) + Run/Submit). Run nəticəsi hər test üçün giriş/gözlənilən/faktiki göstərir; Submit nəticəsi **"Görünən testlər" və "Gizli testlər"** kimi ayrı qruplarda göstərilir (istifadəçi bunu tələb etdi — əvvəlki flat siyahı qarışıq idi) — görünən testlər üçün Run-dakı kimi tam giriş/gözlənilən/nəticə detalı göstərilir (backend datası mövcud idi, əvvəl istifadə olunmurdu), gizli testlər üçün yalnız ✓/✗. XP/Coin/Achievement banner-ləri ilə.
- **Bilinən məhdudiyyət (qəsdən)**: DB-dən gələn content (Challenge/Quiz/Achievement/DailyQuest/Marketplace adları və təsvirləri) yalnız Azərbaycanca seed edilib — AZ/EN keçidi yalnız UI chrome-unu (düymə, etiket, naviqasiya) əhatə edir. İstifadəçiyə bu barədə sual verildi, "olduğu kimi saxla" seçildi.

- **Tətbiq (post-login) UI-nin tam vizual yenidənqurması** — istifadəçinin ətraflı təsvir etdiyi "premium SaaS 2026" spesifikasiyasına görə (Linear/Vercel/GitHub/Raycast ilhamı). **Yalnız Sidebar/Dashboard/Challenges/AppLayout-a aiddir — Login/Register (`AuthLayout`, `BrandingMocks`) əvvəlki "toxunma" kilidinə görə TOXUNULMAYIB.**
  - **Sidebar**: 240px-dən **80px icon-rail**-ə (`w-20`) endirildi — yalnız ikonlar (22px), loqo böyüdüldü (44px, gradient badge), aktiv say `title` atributu ilə tooltip; aktiv menyu = açıq mavi rounded background (əvvəlki glow effekti yox)
  - **Rəng sistemi** (istifadəçinin verdiyi dəqiq hex-lər, `index.css`-də Tailwind v4 `@theme` token kimi): background `#0B1220`(dark)/`#F8FAFC`(light), card `#111827`(dark), accent `#3B82F6` (mavi — əvvəlki indigo-nun yerinə), ikinci aksent `#06B6D4` (cyan, dəyişmədi)
  - **Şrift**: Inter (Google Fonts, `index.html`-ə `<link>` ilə əlavə edildi), `index.css`-də body-ə tətbiq olundu
  - **Glass card sistemi**: `.glass-card` CSS class (`index.css`) + `GlassCard.tsx` komponenti — `border-radius: 20px`, `border: 1px solid rgba(255,255,255,.08)` (dark) / `rgba(15,23,42,.06)` (light), `backdrop-filter: blur(20px)`, shadow. Dashboard stat/panel kartları, Challenges siyahı/detal kartları, ComingSoonPage bunu istifadə edir.
  - **AmbientGlow.tsx** — AppLayout arxa fonunda: tünd gradient fon (`dark:bg-gradient-to-br from-[#0b1220] via-[#0d1526] to-[#0a0f1c]`), zəif grid pattern, 1-2 mavi/cyan glow blob, 2 ədəd solğun üzən kod snippet-i — auth səhifələrindəki `CodeBackdrop` ilə eyni vizual dil, aşağı intensivlikdə (auth səhifələrinin özünə toxunulmadı). "Boş görünür" şikayətinə cavab olaraq istifadəçinin spesifikasiyasına görə qurulub.
    - **Bug (tapılıb düzəldilib)**: ilk versiyada bu effektlər `position:fixed` + `z-index:-10` kombinasiyası ilə tamamilə **görünməz** olurdu (Chromium-da fixed+mənfi z-index elementi gözlənilməz şəkildə render etmirdi/gizlədirdi — istifadəçinin "heçnə dəyişməyib" bildirişi ilə aşkarlandı, DOM/`getComputedStyle` doğru dəyərlər göstərsə də, məcburi qırmızı/sarı test rənglərində belə heç bir piksel çəkilmirdi). **Fix**: mənfi z-index yerine `AmbientGlow`-a `z-0`, məzmun sarğılarına (`Sidebar`, header+main wrapper) isə açıq `relative z-10` verildi — indi stacking sırası mənfi dəyərlərə deyil, aydın müsbət z-index-lərə əsaslanır.
  - Dashboard-a stat kartlarına rəngli ikon badge-ləri əlavə edildi (Level/XP/Coins/Rol), panel başlıqlarına ikon əlavə edildi
  - Playwright ilə hər səhifə (Dashboard/Challenges/ChallengeDetail/ComingSoon) həm light, həm dark modda skrinşotla təsdiqləndi

- **Quiz alma UI** (`features/quiz/{QuizListPage,QuizAttemptPage}.tsx`, route `/practice`, `/practice/:id`) — quiz siyahısı (axtarış+pagination), sual-cavab səhifəsi (bütün suallar bir səhifədə, radio-button seçim, canlı proqres barı, "Bitir" düyməsi yalnız hamısı cavablananda aktivləşir), bir dəfəyə bütün cavabları POST edir (`/api/quizzes/{id}/attempt`). Backend-də vaxt limiti dəstəklənmir (əsl backend məhdudiyyətidir).
- **Quiz nəticə gamification animasiyaları** (istifadəçinin ətraflı spesifikasiyasına görə) — `utils/sounds.ts` (Web Audio API ilə sintez edilmiş success/error/fanfare səsləri, xarici audio fayl lazım deyil), `components/ui/{Confetti,FloatingXp,QuizCompleteModal}.tsx`, `index.css`-də `shake`/`pop-in`/`confetti-fall`/`float-up-fade`/`modal-in` keyframe-ləri. Axın: submit-dən sonra suallar **ardıcıl** (450ms fasilə ilə) "reveal" olunur — hər sual üçün doğrusa yaşıl check + pop-in + success səsi, səhvsə shake + səhv seçim üstündən xətt + doğru cavab yaşılla göstərilir + error səsi; hamısı açılandan sonra fanfare səsi ilə mərkəzi popup açılır (⭐ ulduz reytinqi 1-3, +XP, "Mükəmməl nəticə!" 3/3 olanda), floating "+XP" yuxarı üzür, 3/3 olanda 2 saniyə konfeti yağır. **Qeyd**: Coin animasiyası **yoxdur** — backend Quiz tamamlanmasına Coin vermir (yalnız XP), istifadəçiyə sual verildi, "coin animasiyasını atla" seçildi (backend-ə CoinReward əlavə etmək ayrıca qərar tələb edir, hələ edilməyib).
- **Quiz siyahısında "Tamamlanıb" nişanı** — `QuizListPage`-də istifadəçinin `getMyQuizAttempts` ilə əvvəllər cəhd etdiyi quiz-lərin kartında yaşıl "✓ Tamamlanıb" badge (sağ-üst) və kart altında "Yenidən başla" (əvəzinə "Başla") label göstərilir — istifadəçinin sorğusu ilə.

- **Marketplace UI (Shop)** (`features/shop/ShopPage.tsx`, route `/shop`) — item type tab filtri emoji ilə (🛍️Hamısı/😀Avatar/🏅Badge/💡Hint/🖼️ProfileFrame/🎨Theme/👑Title), hər kart: böyük ikon, ad, təsvir, qiymət, **rarity sistemi** (`utils/rarity.ts` — qiymətə görə avtomatik: <50 Common/50-99 Rare/100-149 Epic/150+ Legendary, kartın kənar xətti rarity rənginə uyğun, istifadəçinin sorğusu ilə qərarlaşdırıldı ki, backend-ə toxunulmasın), "Sahibsən" tam-enli yaşıl zolaq, equip-oluna bilən tiplər üçün "Tax" düyməsi, kifayət qədər coin olmayanda "🔒 Kilidli" düyməsi (basanda "Daha N coin lazımdır" toast-ı göstərir, disabled deyil). Böyük "Wallet" tərzi coin göstəricisi header-də. Hover-də kart bir az böyüyür + rarity rənginə uyğun glow kölgə. Satın alma animasiyası: kart üzərində shimmer+pulse + "🎉 Alındı!" toast-ı, coin sayı `useAnimatedNumber` hook-u ilə əvvəlki-yeni dəyər arasında animasiyalı azalır, success səsi. `authStore.updateUser({coins})` ilə header-dəki balans backend-in qaytardığı `remainingCoins`-dən canlı yenilənir. `authStore`-a `updateUser(patch)` action-u əlavə edildi.

- **Leaderboard səhifəsi** (`features/leaderboard/LeaderboardPage.tsx`, route `/leaderboard`) — dövr tabları (Bu gün/Bu həftə/Bu ay/Bütün zamanlar), top-3 podium (rank1 ortada/böyük+tac, rank2/3 yanlarda, rəngli medal ikonları), qalanlar üçün sıralı siyahı (rank, avatar, istifadəçi adı, level, XP). Cari istifadəçi harada olursa (podium və ya siyahıda) mavi ring + "(Sən)" etiketi ilə vurğulanır. API (`getLeaderboard`) və tiplər (`LeaderboardEntry`/`LeaderboardPeriod`) əvvəlki Dashboard mərhələsində artıq hazırlanmışdı, yalnız UI qurulub.

### Hələ qurulmayıb (növbəti addımlar, prioritet sırası ilə)
1. Achievements səhifəsi
2. Notifications
3. Profile səhifəsi
4. Admin panel (bütün CRUD-lar üçün UI, Excel import/export UI, Audit log)

## Plandan kənarlaşmalar (ARCHITECTURE.md-ə nəzərən)

- `DailyQuestGeneratorJob` (BackgroundService) əvəzinə **lazy generation**
- `LeaderboardSnapshot` cədvəli hazırda **istifadə olunmur** — real-time sorğu (AllTime: UserProfile.Xp, dövri: `XpTransaction` log cədvəli)
- Repository/UnitOfWork pattern əvəzinə **`IApplicationDbContext` birbaşa**
- MediatR **12.5.0**-a sabitlənib (13+ kommersiya lisenziyası)
- Auth səhifələrinin fon kanvası **tema keçidindən asılı olmayaraq həmişə tünd/rəngli** qalır (yalnız forma kartı özü light/dark uyğunlaşır) — Linear/Stripe-in "brand moment" auth səhifəsi konvensiyasına uyğun, qəsdən

## Məlum, düzəldilməyən (aşağı risk) xəbərdarlıqlar

- `Microsoft.OpenApi` NU1903 (upstream, .NET 10 webapi template defolt versiyası)
- `monaco-editor` → `dompurify` npm audit xəbərdarlığı (upstream)

## Test data

DB-də 2 nümunə challenge var (curl ilə admin tərəfindən əlavə edilib, seed data-nın hissəsi deyil): id=1 "A+B Problem" (Arrays/Easy, 2 görünən + 1 gizli test), id=2 "Palindrome Yoxlanışı" (Strings/Medium, 2 görünən test). 1 nümunə quiz: id=1 "C# Əsasları" (3 sual, 40 XP). Bunlar Playwright brauzer testləri üçün istifadə olunur.

## Növbəti addım

Achievements səhifəsi qururam.
