# Hopaut — Product Idea & Concept Overview

> This document describes what Hopaut **is**, what it does, what makes it different, and how it looks and feels. It is written from a product/idea perspective, not a technical one. Developers looking for the technical architecture should consult `Hopaut-Backend-State.md` and `hopaut-solution-fix.md`.

---

## What is Hopaut?

Hopaut is a **geosocial event networking app** — a platform that connects people through real-world events happening near them right now. Think of it as a mashup between a social network and an event board, anchored entirely to your physical location. You open the app, see what is happening around you on a live map, decide what looks interesting, and join. No algorithm decides what you see. No sponsored feed. Just real events, real people, real proximity.

The core idea is simple but powerful: **anything can be an event** — a house party three blocks away, a car meet at the industrial estate, a spontaneous street party, a bicycle ride through the city, a bar night with strangers who share your interests, a sports session in the park. The host posts it, people nearby see it, and the world gets a little more social.

---

## The Problem Hopaut Solves

Modern social networks are excellent at keeping you connected to people you already know but terrible at helping you discover things happening **near you, right now, with people you do not know yet**. Mainstream event platforms (Eventbrite, Facebook Events, Meetup) are either too formal, paywalled, or algorithmically filtered to the point where spontaneous discovery is lost.

Hopaut fills the gap between "I want to do something tonight" and "I want to create a large ticketed event". It is the layer for **informal, organic, proximity-based social interaction** — the kinds of things people used to hear about word-of-mouth.

---

## Core Features

### 1. Live Proximity Map — the Heart of the App
The Search screen is a full-screen, interactive **HERE SDK map** centred on the user's current location. Every active event within the user-defined radius appears as a pin on the map in real time. Users can zoom in, pan around, and tap pins to preview events without leaving the map. A mini floating action button lets users snap the map back to their current GPS position instantly.

This is the feature that defines Hopaut. There is no list-first approach, no "events in your city" — everything is anchored to where you physically stand right now.

### 2. Event Types — Something for Everyone
Events are not generic. When a host creates a post, they choose one of **nine curated event types**, each carrying its own implicit context:

| Type | What it is |
|------|-----------|
| **House Party** | Private gathering, optionally gated by an attendee slot limit |
| **Club** | Night out at a club or nightlife venue |
| **Bar** | Drinks-focused evening at a bar or pub |
| **Biker Meet** | Motorcycle enthusiast meetup |
| **Bicycle Meet** | Cycling group ride or meetup |
| **Car Meet** | Automotive enthusiasts gathering |
| **Street Party** | Open-air community event on public space |
| **Sport** | Any organised sport session — football, running, tennis, anything |
| **Other** | Free-form events that don't fit a category |

Each type is visually distinct in the UI and filterable on the map, so users can instantly narrow down to only the kinds of events they care about.

### 3. Smart Filter Carousel
Above the map, a horizontally scrollable **filter carousel** lets users toggle event types on/off. The map updates in real time as filters change. Users can also search by **hashtag** — the feed returns only posts tagged with that keyword, enabling micro-communities to surface their events through shared vocabulary.

### 4. Attendance Request & Host Control (Privacy by Default)
Attending an event on Hopaut is **not a one-click RSVP**. When a user taps "Attend" they send a join request to the host. The host then reviews a list of pending requests and explicitly **accepts or rejects** each one. For House Parties specifically, the host can define a **maximum slot count** — once capacity is reached, no further requests can be accepted.

When an attendee is accepted, they receive an **instant push notification** confirming they are in. This creates a private, intentional social dynamic: the host curates their guest list, attendees feel genuinely welcomed, and the event stays under control. It mirrors how real social life works — you don't just show up, you get invited.

### 5. Announcements — Live Event Communication
Once an event has attendees, the host can broadcast **announcements** — short messages that are pushed to all confirmed attendees simultaneously. This is the host's megaphone: "bring cash", "parking is around the back", "we're starting 30 minutes late", "there's a second floor open now". Attendees see announcements in a chat-bubble style inbox, grouped per event.

This in-app communication layer means **no need to share personal contacts** (phone numbers, WhatsApp groups) before an event. Hopaut is the channel.

### 6. Ratings & Reputation
After an event ends, attendees can rate the host and leave written feedback. Ratings are tied to a specific event post, attributed to the rater, and rolled up into the host's overall reputation score visible on their public profile.

This creates a **trust layer** that grows over time. Active hosts who run great events build a visible reputation. Bad actors are held accountable. New users can check a host's track record before requesting to join.

### 7. Rich Event Posts with Photos
Every event post can carry a **gallery of photos** — multiple images per post, displayed as a swipeable carousel on the event detail page. Images are uploaded via multipart form, converted to **WebP format** on the client side for efficiency, and stored on **AWS S3**. The host can upload a cover-quality image that represents the vibe of the event: the venue, a flyer, a past edition photo.

Users can tap any photo to open a full-screen image viewer.

### 8. Hashtag Tagging System
Hosts can attach **hashtags** to their posts. Tags are shared across the platform — searching or filtering by a tag like `#techno`, `#sundayride`, or `#5aside` returns all posts tagged with that term globally (within the radius). Tags also appear as chips on the event detail page, tappable for quick discovery.

### 9. User Profiles and Public Reputation Pages
Every user has a public profile accessible to other app users, showing their name, profile picture, a short bio/description, and their aggregated rating score with the number of ratings received. Profiles are the trust passport of the platform.

### 10. Reporting & Moderation
Hopaut has a two-layer reporting system:
- **Report a post** — flag an event for inappropriate content, with a reason code and free-text message. Reports are tied to the specific post and the host.
- **Report a user** — flag a user's behaviour independently of any specific event.

This dual system mirrors how real-world social trust works: sometimes the event itself is the problem, sometimes it is the person.

### 11. In-App Bug Reporting with Screenshots
Users can file bug reports directly from the Settings screen. A bug report captures the message, a timestamp, the reporter's identity, and optionally **one or more screenshots** of what went wrong, uploaded directly from the device. This turns every user into a QA contributor without needing external tooling.

### 12. Payments Integration
Hopaut includes a **payment gateway proxy** — the API tunnels through to an external payment service using a token-based handshake. This positions the platform to support ticketed or paid events (entrance fees, reservations) without the host having to build their own payment flow. The event model already carries an optional `entrancePrice` and `currency` field.

### 13. Facebook Login
Users can authenticate via **Facebook OAuth** as an alternative to the email+password flow, lowering the friction to getting started. The app requests the Facebook access token on the client and exchanges it server-side for a Hopaut JWT.

### 14. Email Confirmation & Password Reset
Standard but important: new accounts require **email confirmation** before the user can log in. Password reset is handled via a tokenised link sent to the registered email. Transactional emails are sent via SMTP and rendered from HTML templates stored on the server.

### 15. OTP Phone Verification (Firebase)
The app integrates **Firebase phone authentication** for OTP verification, adding an extra identity verification step where needed.

### 16. Push Notifications
All significant social triggers on Hopaut fire a **push notification via OneSignal**:
- Attendance request accepted / rejected.
- New announcement from a host.
- (Extensible for: new attendee request received, new rating, new report resolved.)

Notifications carry a `data` payload with the relevant `postId`, so tapping the notification deep-links the user directly to the correct event page.

### 17. Multilingual (English + Russian)
The app ships with full localisation for **English and Russian**, including push notification templates and in-app strings via `easy_localization`. The backend notification templates are bilingual, selecting the correct language per notification.

### 18. Repeatable Events *(scaffolded, in progress)*
The data model includes a `RepeatableProperty` entity with a frequency and end date, designed to support recurring events — a weekly football match, a monthly car meet, a daily yoga class. The infrastructure is in place; the full scheduling feature is on the roadmap.

---

## What Makes Hopaut Different

### Proximity is the Algorithm
On Hopaut, the feed is determined entirely by **physical distance**, not machine learning, paid promotion, or social graph. You see what is near you. A first-time user and a power user see the same events in the same area. Discovery is genuinely democratic.

### Events are Private by Default, Not Public
The attendee-request model means every event is **gated**. This is the opposite of most event platforms where anyone can RSVP. The host decides who gets in. For house parties and intimate gatherings this is essential; it prevents strangers from gatecrashing and gives hosts real control.

### Host-to-Attendee Communication Without Contact Sharing
The announcement system means the host can keep all confirmed attendees informed **without anyone exchanging phone numbers or being added to group chats**. The event is the social unit, and Hopaut is the communication layer for it.

### Nine Opinionated Event Categories
Rather than a blank "create event" form, Hopaut's typed event taxonomy sets context automatically. A BikerMeet has different expectations than a HouseParty. The type becomes part of the social signal — filtering to "CarMeet" immediately builds a niche community inside the broader app.

### Reputation Tied to Real Events
Ratings on Hopaut are not abstract stars — they are attached to specific events and specific interactions between real people who met in person. This makes the reputation system much more meaningful than platform-wide follower counts or likes.

### Real-Time Spatial Discovery
Using **PostGIS with geography point types and `IsWithinDistance` queries** at the database layer, the app delivers true spatial queries rather than bounding-box approximations. You set a radius in kilometres; only events within that precise circle appear. As you move, the feed moves with you.

---

## The User Interface — How it Looks and Feels

### Design Language
Hopaut's visual identity is warm, energetic, and social. The brand palette centres on a **warm sunset gradient** — a left-to-right sweep from a golden-yellow orange (`#ff9e6f`) to a vivid magenta-pink (`#f2326d`). This gradient appears on every app bar, the account page header band, floating action buttons, and primary call-to-action elements. It gives the app a consistent, recognisable warmth that feels evening-social rather than corporate.

The typeface is **Poppins** throughout — geometric, friendly, and modern. Body text is clean, with generous spacing. Headings are concise.

### The Map Screen (Search)
Opening the app takes you straight to a full-screen **live map** rendered by HERE SDK. The map fills the entire screen edge to edge. A compact floating card near the top of the screen contains a search bar (with a subtle vertical divider separating a search icon from the text field) and a horizontally scrolling **filter carousel** with circular checkbox icons for each event type. The card has a white-with-opacity background, softly rounded corners, and a light drop shadow, so it floats above the map without obscuring it. A small floating action button in the bottom-right corner displays a navigation arrow icon — tapping it snaps the map back to your current GPS location.

Event pins dot the map. Tapping a pin opens a bottom-sheet or navigates to the event detail page.

### Event Detail Page
The event detail page is a **scrollable, immersive layout** that opens with a full-width photo carousel at the top (powered by `carousel_pro`). If the event has multiple pictures, you swipe horizontally through them. Below the carousel: the event title and type chip, the host's profile picture and name (tappable to open a profile dialog), the event date/time and location (city, address), and a collapsible description block.

Further down the page: requirements (if any), slot count (for house parties), a tag chip row, and a participants section showing accepted attendees with profile avatars.

Floating over the bottom of the screen is the **Attend button** — a pill-shaped gradient button that animates into view as the user scrolls down past the host section. The button text changes based on state: "Attend", "Requested", or "Attending". For the host, the same area shows an "Edit" and "Delete" control.

### Event List Screen (My Events)
The hosted and attended events screen uses a **NestedScrollView with a tab bar** (Current / Past) to split active and archived events. Each event appears as a `MiniPostCard` — a wide card with a soft multi-layered shadow and a distinctive **continuous elliptical border radius** that gives the left and right edges an organic, slightly curved shape (not just a simple rounded rectangle). The left third of the card is a thumbnail image; the right two-thirds contains the event title, type icon, date, and location in two lines. The card proportions feel modern and distinct from a standard Material list tile.

The floating action button for the My Events tab is a pink FAB with a white plus icon for creating a new event.

### Create / Edit Event Form
The create and edit event flows are **sectioned scrollable forms** with a gradient app bar. Each field is its own card or section: event type selector (custom dropdown), title, description, requirements, start/end datetime pickers (custom time picker widget), entrance price + currency picker, slot count (House Party only), tag input with chip display, and a photo attachment row. The form is clean and stepwise, not a long wall of fields.

### Account / Profile Screen
The account page uses a striking **hero layout**: a full-width gradient band at the top (the same orange-to-pink sunset gradient) that fades into a white card with top-left and top-right rounded corners rising from the bottom. The profile picture floats on the boundary between the gradient and the white card — an overlapping circle avatar with a subtle shadow. Below the profile picture: the user's full name in bold, their description, and then a set of action rows for editing profile details, navigating to hosted/attended events, and accessing settings.

### Settings Screen
The settings page is a clean list-based layout — entries for change password, language, notifications, and delete account. A "Report a Bug" entry at the bottom reinforces Hopaut's commitment to user-driven quality.

### Announcements
The announcements screen renders messages in **chat-bubble style** — right-aligned for outbox (messages the host sent) and left-aligned for inbox (messages received). Timestamps appear below each bubble in a light grey secondary style. The visual rhythm is familiar to any messaging-app user, reducing the learning curve for a new feature.

### Rating Screen
The rating screen is minimal and focused: a large **star rating row** (tappable stars for 1–5) above a multi-line text area for written feedback. A single "Submit" pill button at the bottom. The background uses Hopaut's signature gradient panel behind a white content card.

### Global UI Elements
- **App bars**: always gradient (orange → pink), with white icons and white title text. Platform-adaptive back button (iOS: `arrow_back_ios`, Android: `arrow_back`).
- **Primary buttons**: pill-shaped with the gradient fill, white text in Poppins Semi-Bold. A `PersistButton` variant floats above the keyboard.
- **Inputs**: grey-tinted rounded text fields with a light hint style.
- **Dialogs**: a custom `CustomDialog` and `FullscreenDialog` component used consistently for destructive confirmations and large-content prompts.
- **Loading**: a `LoadingPopup` overlay with an activity indicator, used during async operations to prevent double-submission.
- **Profile dialog**: a mini modal card that appears when tapping a host name anywhere in the app, showing their avatar, name, and rating, with a "View Profile" CTA.

---

## Platform
- **Mobile app**: Flutter (cross-platform iOS and Android)
- **Backend API**: ASP.NET Core Web API on .NET, hosted on a Linux server (`hopout.eu`)
- **Database**: PostgreSQL with PostGIS extension for geospatial queries
- **Image storage**: AWS S3
- **Maps**: HERE SDK
- **Push notifications**: OneSignal
- **Auth**: JWT bearer tokens with refresh, Facebook OAuth, Firebase OTP
- **Email**: SMTP via Gmail relay

---

## Vision

Hopaut's vision is to make **spontaneous real-world socialising as easy as posting a status update**. The technical complexity (spatial queries, real-time push, host-controlled attendance, reputation systems) is hidden entirely behind a warm, simple UI that makes creating or joining a nearby event take less than a minute. The app is built for people who want to live more, stay home less, and meet real people in the real world — guided only by where they are right now.

---

*Document generated from source code analysis of `BingoAPI` (backend) and `BingoMobile` (Flutter frontend). For architecture and refactor plans see `Hopaut-Backend-State.md`, `hopaut-solution-fix.md`, `hopaut-execution-plan.md`.*
