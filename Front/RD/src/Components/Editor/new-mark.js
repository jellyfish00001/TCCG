const tagMark = (tag) => ({
    [tag]: {
      name: tag,
      inclusive: true,
      parseDOM: [
        {
          tag,
        },
      ],
      toDOM: () => [tag, 0],
    },
  });
  
  export { tagMark };