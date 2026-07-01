const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage();
  
  // Create Images directory if it doesn't exist
  const fs = require('fs');
  const dir = 'Docs/Images';
  if (!fs.existsSync(dir)){
      fs.mkdirSync(dir, { recursive: true });
  }

  // Navigate to home page
  await page.goto('http://localhost:5158');
  await page.screenshot({ path: 'Docs/Images/home_page.png' });
  
  // Navigate to login page
  await page.goto('http://localhost:5158/Account/Login');
  await page.screenshot({ path: 'Docs/Images/login_page.png' });

  // Navigate to register page
  await page.goto('http://localhost:5158/Account/Register');
  await page.screenshot({ path: 'Docs/Images/register_page.png' });
  
  await browser.close();
})();
