class CAPluginService {
    constructor(port = 8888) {
        if (!Number.isInteger(port)) {
            throw new Error("Invalid port number");
        }

        this.url = `http://localhost:${port}`;
    }

    async getToken() {
        const response = await fetch(`${this.url}/GetToken`, {
            method: 'GET',
            mode: 'cors',
            headers: {
                'Content-Type': 'text/plain'
            },
            referrerPolicy: "no-referrer"
        });

        if (!response.ok) {
            throw new Error('Get token failed');
        }

        return response.text();
    }

    async getCert(token) {
        if (!token) {
            throw new Error("Token can not be null or empty");
        }

        const data = { token };

        const response = await fetch(`${this.url}/GetCert`, {
            method: 'POST',
            mode: "cors",
            headers: {
                'Content-Type': 'text/plain',
            },
            referrerPolicy: "no-referrer",
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error('Get certificate failed');
        }

        return response.text();
    }

    async signHash(token, base64Hash, serialNumber) {
        if (!token || !base64Hash || !serialNumber) {
            throw new Error("Parameters can not be null or empty");
        }

        const data = {
            token: token,
            data: base64Hash,
            serialNumber: serialNumber
        };

        const response = await fetch(`${this.url}/SignHash`, {
            method: 'POST',
            mode: "cors",
            headers: {
                'Content-Type': 'application/json',
            },
            referrerPolicy: "no-referrer",
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error('Sign hash failed');
        }

        return response.text();
    }

    async signXml(token, xml, serialNumber) {
        if (!token || !xml || !serialNumber) {
            throw new Error("Parameters can not be null or empty");
        }

        const data = {
            token: token,
            data: xml,
            serialNumber: serialNumber
        };

        const response = await fetch(`${this.url}/SignXml`, {
            method: 'POST',
            mode: "cors",
            headers: {
                'Content-Type': 'text/plain',
            },
            referrerPolicy: "no-referrer",
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error('Encrypt data failed');
        }

        return response.text();
    }

    async encrypt(token, message) {
        if (!token || !message) {
            throw new Error("Parameters can not be null or empty");
        }

        const data = {
            token: token,
            data: message,
        };

        const response = await fetch(`${this.url}/Encrypt`, {
            method: 'POST',
            mode: "cors",
            headers: {
                'Content-Type': 'text/plain',
            },
            referrerPolicy: "no-referrer",
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error('Encrypt data failed');
        }

        return response.text();
    }

    async decrypt(token, message) {
        if (!token || !message) {
            throw new Error("Parameters can not be null or empty");
        }

        const data = {
            token: token,
            data: message,
        };

        const response = await fetch(`${this.url}/Decrypt`, {
            method: 'POST',
            mode: "cors",
            headers: {
                'Content-Type': 'text/plain',
            },
            referrerPolicy: "no-referrer",
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error('Encrypt data failed');
        }

        return response.text();
    }
}

export { CAPluginService }