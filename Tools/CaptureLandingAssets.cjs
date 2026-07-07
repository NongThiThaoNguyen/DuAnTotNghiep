const fs = require("node:fs");
const path = require("node:path");
const { chromium } = require("playwright");

const root = path.resolve(__dirname, "..");
const outputDir = path.join(root, "wwwroot/images/landing");
const baseUrl = process.argv[2] || "http://127.0.0.1:5117";

const shots = [
  {
    email: "student1@aistudyenglish.com",
    password: "Password@123",
    url: "/Student",
    file: "student-dashboard-preview.png"
  },
  {
    email: "student1@aistudyenglish.com",
    password: "Password@123",
    url: "/Student/LearningPath",
    file: "student-learning-path-preview.png"
  },
  {
    email: "teacher@aistudyenglish.com",
    password: "Password@123",
    url: "/Teacher/Messages",
    file: "teacher-workspace-preview.png"
  }
];

async function login(page, email, password) {
  await page.goto(`${baseUrl}/Account/Login`, { waitUntil: "networkidle" });
  await page.fill('input[name="Email"]', email);
  await page.fill('input[name="Password"]', password);
  await Promise.all([
    page.waitForNavigation({ waitUntil: "networkidle" }),
    page.click('button[type="submit"]')
  ]);
}

(async () => {
  fs.mkdirSync(outputDir, { recursive: true });

  const browser = await chromium.launch();
  try {
    for (const shot of shots) {
      const context = await browser.newContext({ viewport: { width: 1440, height: 920 }, deviceScaleFactor: 1 });
      const page = await context.newPage();

      await login(page, shot.email, shot.password);
      await page.goto(`${baseUrl}${shot.url}`, { waitUntil: "networkidle" });
      await page.screenshot({
        path: path.join(outputDir, shot.file),
        fullPage: false
      });

      await context.close();
    }
  } finally {
    await browser.close();
  }
})();
