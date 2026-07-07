const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const root = path.resolve(__dirname, "../..");

function read(relativePath) {
  return fs.readFileSync(path.join(root, relativePath), "utf8");
}

test("static files are served before endpoint static asset routing", () => {
  const program = read("Program.cs");
  const useStaticFiles = program.indexOf("app.UseStaticFiles();");
  const useRouting = program.indexOf("app.UseRouting();");
  const mapStaticAssets = program.indexOf("app.MapStaticAssets();");

  assert.notEqual(useStaticFiles, -1);
  assert.ok(useStaticFiles < useRouting);
  assert.ok(useStaticFiles < mapStaticAssets);
});

test("shared avatar fallback assets exist and do not use missing png fallbacks", () => {
  assert.ok(fs.existsSync(path.join(root, "wwwroot/images/default-avatar.svg")));
  assert.ok(fs.existsSync(path.join(root, "wwwroot/images/ai-tutor.svg")));

  const fallbackSources = [
    read("Areas/Teacher/Controllers/MessagesController.cs"),
    read("Areas/Teacher/Views/Students/Index.cshtml"),
    read("Areas/Admin/Controllers/AchievementsController.cs"),
    read("Areas/Admin/Views/Achievements/UserAchievements.cshtml"),
    read("Views/Profile/Index.cshtml"),
    read("Views/AITutor/Index.cshtml"),
    read("wwwroot/js/chat.js")
  ].join("\n");

  assert.doesNotMatch(fallbackSources, /src=["']\/default-images\/avatar\.png|src=["']\/images\/default-avatar\.png|src=["']\/images\/ai-tutor\.png|background=6C63FF/);
  assert.match(fallbackSources, /\/images\/default-avatar\.svg/);
  assert.match(fallbackSources, /\/images\/ai-tutor\.svg/);
});
