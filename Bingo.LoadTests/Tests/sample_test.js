import http from 'k6/http';
import { sleep } from 'k6';

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    vus: 200,
    duration: '10s'

};

export default () => {
    let data =
    {
        email: "administration@hopaut.com",
        password: "String123!"
    };

    http.post(
        'https://hop-out.com/api/v1/identity/login',
        JSON.stringify(data),
        { headers: { 'Content-Type': 'application/json' } }
    )
}