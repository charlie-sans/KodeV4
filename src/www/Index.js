function RenderMainPage() {
    document.body.innerHTML = `
        <div>
            <h1>Welcome to KodeRunner Razor Page</h1>
            <p>This is the main page rendered by the RenderMainPage function.</p>
        </div>
    `;
}

const Functions = {
    RenderMainPage
};

RenderMainPage();