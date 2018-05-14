from lxml import html
import os.path


def normalize(dir, filename):
    fn = os.path.join(dir, filename)
    doc = html.parse(fn)
    s = html.tostring(doc, encoding=str, pretty_print=True)
    nnfn = os.path.join(dir, "norm-"+filename)
    o = open(nnfn, "w")
    o.write(s)
    o.close()


def main():
    normalize("../views/", "home.html")


if __name__ == "__main__":
    main()
