export type PublicCourse = {
    id: number;
    title: string;
    description: string;
    thumbnailUrl: string;
    thumbnailAltText: string;
    totalLessons: number;
};
function isPublicCourse(value: unknown): value is PublicCourse {
    if (!value || typeof value !== "object") return false;
    const course = value as Record<string, unknown>;
    return Number.isSafeInteger(course.id) && Number(course.id) > 0
        && typeof course.title === "string" && typeof course.description === "string"
        && typeof course.totalLessons === "number" && Number.isInteger(course.totalLessons) && course.totalLessons >= 0;
}
export function learningUrl() {
    const configured = process.env.BACKEND_URL;
    if (!configured)
        return null;
    try {
        const url = new URL(configured);
        if (!["https:", "http:"].includes(url.protocol))
            return null;
        return new URL("/courses", url).toString();
    }
    catch {
        return null;
    }
}
async function request(path: string) {
    if (!process.env.BACKEND_URL)
        throw new Error("Course backend is not configured");
    return fetch(`${process.env.BACKEND_URL.replace(/\/$/, "")}/api/courses${path}`, { cache: "no-store", headers: { Accept: "application/json" }, signal: AbortSignal.timeout(10000) });
}
export async function fetchPublicCourses(): Promise<{
    courses: PublicCourse[];
    error: boolean;
}> {
    try {
        const response = await request("");
        if (!response.ok)
            throw new Error("Course request failed");
        const courses = await response.json();
        if (!Array.isArray(courses) || !courses.every(isPublicCourse))
            throw new Error("Invalid course response");
        return { courses, error: false };
    }
    catch {
        return { courses: [], error: true };
    }
}
export async function fetchPublicCourse(id: number): Promise<{
    course: PublicCourse | null;
    error: boolean;
}> {
    try {
        const response = await request(`/${id}`);
        if (response.status === 404)
            return { course: null, error: false };
        if (!response.ok)
            throw new Error("Course request failed");
        const course: unknown = await response.json();
        if (!isPublicCourse(course)) throw new Error("Invalid course response");
        return { course, error: false };
    }
    catch {
        return { course: null, error: true };
    }
}
