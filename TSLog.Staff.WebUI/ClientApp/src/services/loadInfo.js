export default class LoadInfo {
    async getResource(url) {
        const res = await fetch(url);
        if (!res.ok) {
            throw new Error(`Could not fetch ${url}` +
                `, received ${res.status}`)
        }
        return await res.json();
    }

    async getProfessions() {
        const res = await this.getResource('/api/v1/Staff/Position');
        return res
    }
}

const professions = new LoadInfo();

professions.getProfessions().then((body) => {
    console.log(body);
});