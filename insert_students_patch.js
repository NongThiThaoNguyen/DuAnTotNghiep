const http = require('http');

const options = {
  hostname: 'localhost',
  port: 5158,
  path: '/api/MigrationPatch/run-insert-students',
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Content-Length': 0
  }
};

console.log("Bắt đầu gọi API để tạo 100 học viên...");

const req = http.request(options, (res) => {
  let data = '';

  res.on('data', (chunk) => {
    data += chunk;
  });

  res.on('end', () => {
    console.log(`Status Code: ${res.statusCode}`);
    console.log(`Response: ${data}`);
    console.log("Hoàn thành quá trình thêm 100 học viên.");
  });
});

req.on('error', (error) => {
  console.error("Lỗi khi gọi API:", error.message);
});

req.end();
