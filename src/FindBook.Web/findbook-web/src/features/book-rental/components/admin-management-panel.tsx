"use client";

import { useEffect, useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { FileUpload, type FileUploadSelectEvent } from "primereact/fileupload";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useAppForm } from "@/hooks/use-app-form";
import {
  type AddressFormInput,
  type AdminUserFormInput,
  adminUserFormSchema,
  type BookFormInput,
  bookFormSchema,
  type CategoryFormInput,
  categoryFormSchema,
  type DeliveryTaskFormInput,
  deliveryTaskFormSchema,
  type LibraryFormInput,
  libraryFormSchema,
} from "@/features/book-rental/schemas/book-rental-forms.schema";
import {
  completeDeliveryTask,
  createBook,
  createCategory,
  createDeliveryTask,
  createLibrary,
  createUser,
  failDeliveryTask,
  markDeliveryTaskInTransit,
  uploadBookCover,
  updateBook,
  updateCategory,
  updateLibrary,
  updateUser,
} from "@/features/book-rental/services/book-rental.client";
import type {
  AdminOverviewData,
  ApiBook,
  ApiCategory,
  ApiLibrary,
  ApiUser,
} from "@/features/book-rental/types/book-rental.types";

type AdminManagementPanelProps = {
  data: AdminOverviewData;
};

export function AdminManagementPanel({ data }: AdminManagementPanelProps) {
  return (
    <section className="admin-forms-grid">
      <UserManager libraries={data.libraries} users={data.users} />
      <CategoryManager categories={data.categories} />
      <LibraryManager libraries={data.libraries} />
      <BookManager books={data.books} categories={data.categories} libraries={data.libraries} />
      <DeliveryTaskManager data={data} />
    </section>
  );
}

export function AdminUserManagementPanel({
  libraries,
  users,
}: Pick<AdminOverviewData, "libraries" | "users">) {
  return <UserManager libraries={libraries} users={users} />;
}

export function AdminCategoryManagementPanel({
  categories,
}: Pick<AdminOverviewData, "categories">) {
  return <CategoryManager categories={categories} />;
}

export function AdminLibraryManagementPanel({
  libraries,
}: Pick<AdminOverviewData, "libraries">) {
  return <LibraryManager libraries={libraries} />;
}

export function AdminBookManagementPanel({
  books,
  categories,
  libraries,
}: Pick<AdminOverviewData, "books" | "categories" | "libraries">) {
  return <BookManager books={books} categories={categories} libraries={libraries} />;
}

export function AdminDeliveryTaskManagementPanel({
  data,
}: AdminManagementPanelProps) {
  return <DeliveryTaskManager data={data} />;
}

function UserManager({
  libraries,
  users,
}: {
  libraries: ApiLibrary[];
  users: ApiUser[];
}) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [selectedId, setSelectedId] = useState<number | "new">("new");
  const [serverError, setServerError] = useState<string | null>(null);
  const form = useAppForm({
    defaultValues: emptyUserValues(),
    schema: adminUserFormSchema,
  });

  const selectedUser = selectedId === "new" ? null : users.find((item) => item.id === selectedId) ?? null;

  const submit = form.handleSubmit(async (data) => {
    setServerError(null);

    try {
      if (selectedUser) {
        await updateUser(selectedUser.id, data as unknown as AdminUserFormInput);
      } else {
        await createUser(data as unknown as AdminUserFormInput);
      }

      startTransition(() => {
        setSelectedId("new");
        form.reset(emptyUserValues());
        router.refresh();
      });
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to save user.");
    }
  });

  return (
    <section className="chart-card">
      <FormHeader title="Manage Users" />
      <label className="field">
        <span className="field-label">User to edit</span>
        <select
          className="field-input"
          onChange={(event) => {
            const nextValue = event.target.value;
            const user = users.find((item) => item.id === Number(nextValue));
            setSelectedId(nextValue === "new" ? "new" : Number(nextValue));
            form.reset(
              user
                ? {
                    email: user.email,
                    fullName: user.fullName,
                    managedLibraryId: user.managedLibraryId ?? null,
                    passwordHash: "",
                    phoneNumber: user.phoneNumber ?? "",
                    role: user.role,
                  }
                : emptyUserValues(),
            );
          }}
          value={selectedId}
        >
          <option value="new">Create new user</option>
          {users.map((user) => (
            <option key={user.id} value={user.id}>
              {user.fullName}
            </option>
          ))}
        </select>
      </label>
      <form className="stack-form" onSubmit={submit}>
        <Input error={form.formState.errors.fullName?.message} label="Full Name" {...form.register("fullName")} />
        <Input error={form.formState.errors.email?.message} label="Email" {...form.register("email")} />
        {!selectedUser ? (
          <Input error={form.formState.errors.passwordHash?.message} label="Password Hash" {...form.register("passwordHash")} />
        ) : null}
        <Input error={form.formState.errors.phoneNumber?.message} label="Phone Number" {...form.register("phoneNumber")} />
        <label className="field">
          <span className="field-label">Role</span>
          <select className="field-input" {...form.register("role")}>
            <option value="User">User</option>
            <option value="Admin">Admin</option>
            <option value="SuperAdmin">SuperAdmin</option>
            <option value="DeliveryPartner">DeliveryPartner</option>
          </select>
        </label>
        <label className="field">
          <span className="field-label">Managed Library</span>
          <select className="field-input" {...form.register("managedLibraryId", { setValueAs: (value) => value ? Number(value) : null })}>
            <option value="">No managed library</option>
            {libraries.map((library) => (
              <option key={library.id} value={library.id}>
                {library.name}
              </option>
            ))}
          </select>
        </label>
        {serverError ? <div className="form-alert">{serverError}</div> : null}
        <Button disabled={isPending || form.formState.isSubmitting} type="submit">
          {selectedUser ? "Update User" : "Create User"}
        </Button>
      </form>
    </section>
  );
}

function CategoryManager({ categories }: { categories: ApiCategory[] }) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [selectedId, setSelectedId] = useState<number | "new">("new");
  const [serverError, setServerError] = useState<string | null>(null);
  const form = useAppForm({
    defaultValues: { name: "" },
    schema: categoryFormSchema,
  });

  const selectedCategory = selectedId === "new" ? null : categories.find((item) => item.id === selectedId) ?? null;

  const submit = form.handleSubmit(async (data) => {
    setServerError(null);

    try {
      if (selectedCategory) {
        await updateCategory(selectedCategory.id, data as unknown as CategoryFormInput);
      } else {
        await createCategory(data as unknown as CategoryFormInput);
      }

      startTransition(() => {
        setSelectedId("new");
        form.reset({ name: "" });
        router.refresh();
      });
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to save category.");
    }
  });

  return (
    <section className="chart-card">
      <FormHeader title="Manage Categories" />
      <label className="field">
        <span className="field-label">Category to edit</span>
        <select
          className="field-input"
          onChange={(event) => {
            const nextValue = event.target.value;
            const category = categories.find((item) => item.id === Number(nextValue));
            setSelectedId(nextValue === "new" ? "new" : Number(nextValue));
            form.reset({ name: category?.name ?? "" });
          }}
          value={selectedId}
        >
          <option value="new">Create new category</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.name}
            </option>
          ))}
        </select>
      </label>
      <form className="stack-form" onSubmit={submit}>
        <Input error={form.formState.errors.name?.message} label="Name" {...form.register("name")} />
        {serverError ? <div className="form-alert">{serverError}</div> : null}
        <Button disabled={isPending || form.formState.isSubmitting} type="submit">
          {selectedCategory ? "Update Category" : "Create Category"}
        </Button>
      </form>
    </section>
  );
}

function LibraryManager({ libraries }: { libraries: ApiLibrary[] }) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [selectedId, setSelectedId] = useState<number | "new">("new");
  const [serverError, setServerError] = useState<string | null>(null);
  const form = useAppForm({
    defaultValues: emptyLibraryValues(),
    schema: libraryFormSchema,
  });

  const selectedLibrary = selectedId === "new" ? null : libraries.find((item) => item.id === selectedId) ?? null;

  const submit = form.handleSubmit(async (data) => {
    setServerError(null);

    try {
      if (selectedLibrary) {
        await updateLibrary(selectedLibrary.id, data as unknown as LibraryFormInput);
      } else {
        await createLibrary(data as unknown as LibraryFormInput);
      }

      startTransition(() => {
        setSelectedId("new");
        form.reset(emptyLibraryValues());
        router.refresh();
      });
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to save library.");
    }
  });

  return (
    <section className="chart-card">
      <FormHeader title="Manage Libraries" />
      <label className="field">
        <span className="field-label">Library to edit</span>
        <select
          className="field-input"
          onChange={(event) => {
            const nextValue = event.target.value;
            const library = libraries.find((item) => item.id === Number(nextValue));
            setSelectedId(nextValue === "new" ? "new" : Number(nextValue));
            form.reset(
              library
                ? {
                    address: {
                      city: library.address.city,
                      country: library.address.country,
                      postalCode: library.address.postalCode,
                      state: library.address.state,
                      street: library.address.street,
                    },
                    contactEmail: library.contactEmail,
                    contactPhone: library.contactPhone,
                    name: library.name,
                  }
                : emptyLibraryValues(),
            );
          }}
          value={selectedId}
        >
          <option value="new">Create new library</option>
          {libraries.map((library) => (
            <option key={library.id} value={library.id}>
              {library.name}
            </option>
          ))}
        </select>
      </label>
      <form className="stack-form" onSubmit={submit}>
        <Input error={form.formState.errors.name?.message} label="Name" {...form.register("name")} />
        <Input error={form.formState.errors.contactEmail?.message} label="Email" {...form.register("contactEmail")} />
        <Input error={form.formState.errors.contactPhone?.message} label="Phone" {...form.register("contactPhone")} />
        <Input error={form.formState.errors.address?.street?.message} label="Street" {...form.register("address.street")} />
        <Input error={form.formState.errors.address?.city?.message} label="City" {...form.register("address.city")} />
        <Input error={form.formState.errors.address?.state?.message} label="State" {...form.register("address.state")} />
        <Input error={form.formState.errors.address?.postalCode?.message} label="Postal Code" {...form.register("address.postalCode")} />
        <Input error={form.formState.errors.address?.country?.message} label="Country" {...form.register("address.country")} />
        {serverError ? <div className="form-alert">{serverError}</div> : null}
        <Button disabled={isPending || form.formState.isSubmitting} type="submit">
          {selectedLibrary ? "Update Library" : "Create Library"}
        </Button>
      </form>
    </section>
  );
}

function BookManager({
  books,
  categories,
  libraries,
}: {
  books: ApiBook[];
  categories: ApiCategory[];
  libraries: ApiLibrary[];
}) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [selectedId, setSelectedId] = useState<number | "new">("new");
  const [selectedCoverFile, setSelectedCoverFile] = useState<File | null>(null);
  const [coverPreviewUrl, setCoverPreviewUrl] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const form = useAppForm({
    defaultValues: emptyBookValues(),
    schema: bookFormSchema,
  });

  const selectedBook = selectedId === "new" ? null : books.find((item) => item.id === selectedId) ?? null;

  useEffect(() => {
    if (!selectedCoverFile) {
      setCoverPreviewUrl(selectedBook ? `/api/v1/books/${selectedBook.id}/cover` : null);
      return;
    }

    const objectUrl = URL.createObjectURL(selectedCoverFile);
    setCoverPreviewUrl(objectUrl);

    return () => {
      URL.revokeObjectURL(objectUrl);
    };
  }, [selectedBook, selectedCoverFile]);

  const submit = form.handleSubmit(async (data) => {
    setServerError(null);

    try {
      const savedBook = selectedBook
        ? await updateBook(selectedBook.id, data as unknown as BookFormInput)
        : await createBook(data as unknown as BookFormInput);

      if (selectedCoverFile) {
        await uploadBookCover(savedBook.id, selectedCoverFile);
      }

      startTransition(() => {
        setSelectedId("new");
        setSelectedCoverFile(null);
        setCoverPreviewUrl(null);
        form.reset(emptyBookValues());
        router.refresh();
      });
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to save book.");
    }
  });

  return (
    <section className="chart-card">
      <FormHeader title="Manage Books" />
      <label className="field">
        <span className="field-label">Book to edit</span>
        <select
          className="field-input"
          onChange={(event) => {
            const nextValue = event.target.value;
            const book = books.find((item) => item.id === Number(nextValue));
            setSelectedId(nextValue === "new" ? "new" : Number(nextValue));
            setSelectedCoverFile(null);
            form.reset(
              book
                ? {
                    author: book.author,
                    availableCopies: book.availableCopies,
                    categoryId: book.categoryId,
                    coverImageReference: book.coverImageReference ?? "",
                    description: book.description ?? "",
                    isbn: book.isbn,
                    libraryId: book.libraryId,
                    title: book.title,
                    totalCopies: book.totalCopies,
                  }
                : emptyBookValues(),
            );
          }}
          value={selectedId}
        >
          <option value="new">Create new book</option>
          {books.map((book) => (
            <option key={book.id} value={book.id}>
              {book.title}
            </option>
          ))}
        </select>
      </label>
      <form className="stack-form" onSubmit={submit}>
        <Input error={form.formState.errors.title?.message} label="Title" {...form.register("title")} />
        <Input error={form.formState.errors.author?.message} label="Author" {...form.register("author")} />
        <Input error={form.formState.errors.isbn?.message} label="ISBN" {...form.register("isbn")} />
        <label className="field">
          <span className="field-label">Library</span>
          <select className="field-input" {...form.register("libraryId", { valueAsNumber: true })}>
            <option value="">Choose a library</option>
            {libraries.map((library) => (
              <option key={library.id} value={library.id}>
                {library.name}
              </option>
            ))}
          </select>
          {form.formState.errors.libraryId?.message ? <span className="field-error">{form.formState.errors.libraryId.message}</span> : null}
        </label>
        <label className="field">
          <span className="field-label">Category</span>
          <select className="field-input" {...form.register("categoryId", { valueAsNumber: true })}>
            <option value="">Choose a category</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
          {form.formState.errors.categoryId?.message ? <span className="field-error">{form.formState.errors.categoryId.message}</span> : null}
        </label>
        <Input error={form.formState.errors.totalCopies?.message} label="Total Copies" type="number" {...form.register("totalCopies", { valueAsNumber: true })} />
        <Input error={form.formState.errors.availableCopies?.message} label="Available Copies" type="number" {...form.register("availableCopies", { valueAsNumber: true })} />
        <div className="field">
          <span className="field-label">Cover Upload</span>
          {coverPreviewUrl ? (
            <div className="book-cover-upload-preview">
              <img
                alt={selectedBook ? `${selectedBook.title} cover` : "Selected cover preview"}
                className="book-cover-upload-image"
                src={coverPreviewUrl}
              />
            </div>
          ) : (
            <div className="book-cover-upload-empty">
              Upload a JPEG, PNG, or WEBP cover image up to 5 MB.
            </div>
          )}
          <FileUpload
            accept="image/jpeg,image/png,image/webp"
            cancelOptions={{ className: "book-cover-uploader-hidden" }}
            chooseLabel={selectedCoverFile ? "Replace Cover" : "Select Cover"}
            className="book-cover-uploader"
            customUpload
            key={`cover-${selectedId}-${selectedCoverFile?.name ?? "empty"}`}
            maxFileSize={5 * 1024 * 1024}
            mode="advanced"
            multiple={false}
            name="file"
            onClear={() => {
              setSelectedCoverFile(null);
            }}
            onSelect={(event: FileUploadSelectEvent) => {
              const nextFile = event.files[0];
              setSelectedCoverFile(nextFile instanceof File ? nextFile : null);
            }}
            uploadOptions={{ className: "book-cover-uploader-hidden" }}
          />
          <span className="field-help">
            {selectedBook
              ? "Choose a file to replace the current cover after saving."
              : "For a new book, the cover will upload right after the book record is created."}
          </span>
        </div>
        <Textarea error={form.formState.errors.description?.message} label="Description" rows={4} {...form.register("description")} />
        {serverError ? <div className="form-alert">{serverError}</div> : null}
        <Button disabled={isPending || form.formState.isSubmitting} type="submit">
          {selectedBook
            ? selectedCoverFile
              ? "Update Book and Cover"
              : "Update Book"
            : selectedCoverFile
              ? "Create Book and Upload Cover"
              : "Create Book"}
        </Button>
      </form>
    </section>
  );
}

function DeliveryTaskManager({ data }: AdminManagementPanelProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [serverError, setServerError] = useState<string | null>(null);
  const form = useAppForm({
    defaultValues: {
      deliveryPartnerAccountId: data.deliveryPartners[0]?.id ?? 0,
      rentalId: data.rentals[0]?.id ?? 0,
      type: "Delivery",
    },
    schema: deliveryTaskFormSchema,
  });

  const submit = form.handleSubmit(async (values) => {
    setServerError(null);

    try {
      await createDeliveryTask(values as unknown as DeliveryTaskFormInput);
      startTransition(() => router.refresh());
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to create delivery task.");
    }
  });

  return (
    <section className="chart-card">
      <FormHeader title="Manage Delivery Tasks" />
      <form className="stack-form" onSubmit={submit}>
        <label className="field">
          <span className="field-label">Rental</span>
          <select className="field-input" {...form.register("rentalId", { valueAsNumber: true })}>
            {data.rentals.map((rental) => (
              <option key={rental.id} value={rental.id}>
                Rental #{rental.id} · Book {rental.bookId}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field-label">Delivery Partner</span>
          <select className="field-input" {...form.register("deliveryPartnerAccountId", { valueAsNumber: true })}>
            {data.deliveryPartners.map((partner) => (
              <option key={partner.id} value={partner.id}>
                {partner.fullName}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field-label">Task Type</span>
          <select className="field-input" {...form.register("type")}>
            <option value="Delivery">Delivery</option>
            <option value="Pickup">Pickup</option>
          </select>
        </label>
        {serverError ? <div className="form-alert">{serverError}</div> : null}
        <Button disabled={isPending || form.formState.isSubmitting} type="submit">
          Create Delivery Task
        </Button>
      </form>

      <div className="stack-list" style={{ marginTop: "20px" }}>
        {data.deliveryTasks.map((task) => (
          <div className="address-card" key={task.id}>
            <div>
              <div style={{ fontWeight: 700 }}>
                Task #{task.id} · {task.type}
              </div>
              <div className="subtle-text">
                Rental {task.rentalId} · {task.status}
              </div>
            </div>
            <div className="inline-actions">
              {task.status === "Assigned" ? (
                <Button
                  disabled={isPending}
                  onClick={async () => {
                    setServerError(null);
                    try {
                      await markDeliveryTaskInTransit(task.id);
                      startTransition(() => router.refresh());
                    } catch (error) {
                      setServerError(error instanceof Error ? error.message : "Unable to move task in transit.");
                    }
                  }}
                  variant="outline"
                >
                  Mark In Transit
                </Button>
              ) : null}
              {task.status !== "Completed" && task.status !== "Failed" ? (
                <>
                  <Button
                    disabled={isPending}
                    onClick={async () => {
                      setServerError(null);
                      try {
                        await completeDeliveryTask(task.id);
                        startTransition(() => router.refresh());
                      } catch (error) {
                        setServerError(error instanceof Error ? error.message : "Unable to complete task.");
                      }
                    }}
                  >
                    Complete
                  </Button>
                  <Button
                    disabled={isPending}
                    onClick={async () => {
                      setServerError(null);
                      try {
                        await failDeliveryTask(task.id);
                        startTransition(() => router.refresh());
                      } catch (error) {
                        setServerError(error instanceof Error ? error.message : "Unable to fail task.");
                      }
                    }}
                    variant="ghost"
                  >
                    Fail
                  </Button>
                </>
              ) : null}
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}

function emptyLibraryValues(): LibraryFormInput {
  return {
    address: {
      city: "",
      country: "India",
      postalCode: "",
      state: "",
      street: "",
    },
    contactEmail: "",
    contactPhone: "",
    name: "",
  };
}

function emptyBookValues(): BookFormInput {
  return {
    author: "",
    availableCopies: 1,
    categoryId: 0,
    coverImageReference: "",
    description: "",
    isbn: "",
    libraryId: 0,
    title: "",
    totalCopies: 1,
  };
}

function emptyUserValues(): AdminUserFormInput {
  return {
    email: "",
    fullName: "",
    managedLibraryId: null,
    passwordHash: "",
    phoneNumber: "",
    role: "User",
  };
}

function FormHeader({ title }: { title: string }) {
  return <div className="chart-title">{title}</div>;
}
