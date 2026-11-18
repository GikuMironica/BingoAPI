import http from 'k6/http';

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '2m', target: 100 },
        { duration: '5m', target: 100},
        { duration: '2m', target: 200},
        { duration: '5m', target: 100},
        { duration: '2m', target: 300},
        { duration: '5m', target: 300},
        { duration: '2m', target: 400},
        { duration: '5m', target: 400},
        { duration: '10m', target: 0}
    ]
}

const API_BASE_URL = 'https://localhost:44375';

export default () => {
    http.batch([
        ['GET', `${API_BASE_URL}/youtube`],
        ['GET', `${API_BASE_URL}/github`],
        ['GET', `${API_BASE_URL}/twitter`],
    ]);
}