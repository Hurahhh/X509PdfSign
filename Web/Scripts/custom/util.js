const util = {
    base64ToArrayBuffer(base64) {
        const binary_string = window.atob(base64);
        const len = binary_string.length;
        const bytes = new Uint8Array(len);

        for (let i = 0; i < len; i++) {
            bytes[i] = binary_string.charCodeAt(i);
        }

        return bytes.buffer;
    },

    pemFromBase64Cert(cert) {
        const cert_begin = "-----BEGIN CERTIFICATE-----\n";
        const end_cert = "\n-----END CERTIFICATE-----";

        return cert_begin + cert + end_cert;
    }
}